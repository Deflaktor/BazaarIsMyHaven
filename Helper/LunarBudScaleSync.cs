using RoR2;
using RoR2.Networking;
using RoR2.Scripts.GameBehaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace BazaarIsMyHaven
{
    // Spawned buds keep the prefab's half scale on clients, since Unity does not sync scale.
    // Clients are fixed with vanilla messages only: a Drifter hoard's RpcParentToMuzzle sets a bud's scale to 1
    // (and moves it to the world origin); a teleport moves it back and a refresh rebuilds the model.
    public class LunarBudScaleSync : MonoBehaviour
    {
        private static AsyncOperationHandle<GameObject> helperPrefab;
        private static bool helperUnavailable;

        private readonly List<NetworkIdentity> buds = new List<NetworkIdentity>();
        private readonly HashSet<NetworkConnection> synchronizedClients = new HashSet<NetworkConnection>();

        public static void Preload()
        {
            helperPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/Drifter/DrifterHoard.prefab");
        }

        public static void Apply(List<GameObject> budObjects)
        {
            if (!NetworkServer.active || budObjects.Count == 0 || helperUnavailable)
            {
                return;
            }

            var syncObject = new GameObject("LunarBudScaleSync");
            SceneManager.MoveGameObjectToScene(syncObject, budObjects[0].scene);
            var sync = syncObject.AddComponent<LunarBudScaleSync>();
            foreach (var bud in budObjects)
            {
                sync.buds.Add(bud.GetComponent<NetworkIdentity>());
            }
            sync.StartCoroutine(sync.SynchronizeClients());
        }

        private IEnumerator SynchronizeClients()
        {
            var retryDelay = new WaitForSecondsRealtime(1f);

            // clients are ready after the scene loads, and players can join late
            while (NetworkServer.active)
            {
                buds.RemoveAll(bud => !bud);
                if (buds.Count == 0)
                {
                    break;
                }

                synchronizedClients.RemoveWhere(connection => !connection.isReady || !NetworkServer.connections.Contains(connection));

                var pendingClients = GetPendingClients();
                bool sent = true;
                if (pendingClients.Count > 0)
                {
                    try
                    {
                        sent = SendScaleFix(pendingClients);
                    }
                    catch (Exception exception)
                    {
                        helperUnavailable = true;
                        Log.LogError($"Could not fix bud scale for clients: {exception}");
                        break;
                    }
                }

                // a full queue must not spawn a helper every frame
                yield return sent ? null : retryDelay;
            }

            Destroy(gameObject);
        }

        private List<NetworkConnection> GetPendingClients()
        {
            var result = new List<NetworkConnection>();
            foreach (var connection in NetworkServer.connections)
            {
                // the host already has the right scale
                if (connection == null || !connection.isReady || Util.ConnectionIsLocal(connection) || synchronizedClients.Contains(connection))
                {
                    continue;
                }
                // the client needs every bud spawned before messages can target them
                if (buds.All(bud => bud.observers != null && bud.observers.Contains(connection)))
                {
                    result.Add(connection);
                }
            }
            return result;
        }

        private bool SendScaleFix(List<NetworkConnection> clients)
        {
            var prefab = helperPrefab.WaitForCompletion();
            var itemShare = prefab ? prefab.GetComponent<ItemShareController>() : null;

            // an attachment point or offset would move the buds elsewhere
            if (!itemShare || itemShare.pickupMuzzle || !string.IsNullOrEmpty(itemShare.pickupMuzzleChildName) || itemShare.subsequentPickupLocalOffset != Vector3.zero)
            {
                throw new InvalidOperationException("The Drifter hoard no longer has the expected settings.");
            }

            var helper = Instantiate(prefab, new Vector3(0f, 10000f, 0f), Quaternion.identity);
            bool allSent = true;
            try
            {
                // keep the helper idle; its state also stops the hoard's timer on clients
                helper.GetComponent<EntityStateMachine>().SetState(new EntityStates.Idle());
                helper.GetComponent<BuffWard>().Networkradius = 0f;
                NetworkServer.Spawn(helper);

                var helperIdentity = helper.GetComponent<NetworkIdentity>();
                foreach (var connection in clients)
                {
                    bool sent = true;
                    foreach (var bud in buds)
                    {
                        // each player may have their own stock, so build the refresh per client
                        var refresh = InstancedPurchases.CreatePickupDisplayRefresh(bud, connection);
                        if (refresh == null)
                        {
                            sent = false;
                            continue;
                        }

                        var messages = CreateScaleFixMessages(helperIdentity.netId, bud, refresh);
                        sent = connection.SendBytes(messages, messages.Length, QosChannelIndex.defaultReliable.intVal) && sent;
                    }

                    if (sent)
                    {
                        synchronizedClients.Add(connection);
                    }
                    else
                    {
                        allSent = false;
                    }
                }
            }
            finally
            {
                // the RPC unparents the buds, so destroying the helper leaves them alone
                NetworkServer.Destroy(helper);
            }

            return allSent;
        }

        // The helper's spawn message and these share the reliable channel, so they arrive in order.
        private static byte[] CreateScaleFixMessages(NetworkInstanceId helperId, NetworkIdentity bud, byte[] displayRefresh)
        {
            var scaleWriter = new NetworkWriter();
            scaleWriter.StartMessage(MsgType.Rpc);
            scaleWriter.WritePackedUInt32(unchecked((uint)ItemShareController.kRpcRpcParentToMuzzle));
            scaleWriter.Write(helperId);
            scaleWriter.Write(bud.netId);
            scaleWriter.FinishMessage();

            // the RPC also resets position and rotation, so move the bud back
            var teleportWriter = new NetworkWriter();
            teleportWriter.StartMessage(UmsgType.Teleport);
            new TeleportHelper.TeleportMessage
            {
                gameObject = bud.gameObject,
                newPosition = bud.transform.position,
                newRotation = bud.transform.rotation,
                useRotation = true,
                delta = bud.transform.position
            }.Serialize(teleportWriter);
            teleportWriter.FinishMessage();

            var writer = new NetworkWriter();
            var scaleBytes = scaleWriter.ToArray();
            var teleportBytes = teleportWriter.ToArray();
            writer.Write(scaleBytes, scaleBytes.Length);
            writer.Write(teleportBytes, teleportBytes.Length);
            writer.Write(displayRefresh, displayRefresh.Length);
            return writer.ToArray();
        }
    }
}
