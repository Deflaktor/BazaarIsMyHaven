using RoR2;
using RoR2.Hologram;
using RoR2.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace BazaarIsMyHaven
{
    // Equipment terminals have no price display; in vanilla it belongs to the multishop around them.
    // Each terminal gets a Lunar Seer, whose price sign is not part of its model. Clients are set up with vanilla
    // messages only: a Wisp death state removes the Seer's model and collider, and a teleport moves the sign above the terminal.
    public class EquipmentPriceSigns : MonoBehaviour
    {
        // the vanilla multishop shows its price this high above its terminals
        private const float SignHeight = 5.25f;
        // clients spawn the Seer here, out of sight until its model is gone
        private static readonly Vector3 SetupPosition = new Vector3(0f, 10000f, 0f);

        private static AsyncOperationHandle<GameObject> seerPrefab;
        private static bool signsUnavailable;

        private readonly List<Sign> signs = new List<Sign>();

        private class Sign
        {
            public GameObject terminal;
            public NetworkIdentity seer;
            public Vector3 clientPosition;
            public Quaternion clientRotation;
            public readonly HashSet<NetworkConnection> synchronizedClients = new HashSet<NetworkConnection>();
        }

        public static void Preload()
        {
            seerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/SeerStation.prefab");
        }

        public static void Apply(List<GameObject> terminals)
        {
            if (!NetworkServer.active || terminals.Count == 0 || ModConfig.EquipmentCost.Value <= 0 || signsUnavailable)
            {
                return;
            }

            var syncObject = new GameObject("EquipmentPriceSigns");
            SceneManager.MoveGameObjectToScene(syncObject, terminals[0].scene);
            var priceSigns = syncObject.AddComponent<EquipmentPriceSigns>();
            try
            {
                foreach (var terminal in terminals)
                {
                    priceSigns.signs.Add(CreateSign(terminal));
                }
            }
            catch (Exception exception)
            {
                signsUnavailable = true;
                Log.LogError($"Could not create equipment price signs: {exception}");
                foreach (var sign in priceSigns.signs)
                {
                    NetworkServer.Destroy(sign.seer.gameObject);
                }
                Destroy(syncObject);
                return;
            }
            priceSigns.StartCoroutine(priceSigns.SynchronizeClients());
        }

        private static Sign CreateSign(GameObject terminal)
        {
            var seer = Instantiate(seerPrefab.WaitForCompletion(), SetupPosition, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(seer, terminal.scene);
            try
            {
                var modelLocator = seer.GetComponent<ModelLocator>();
                var projector = seer.GetComponent<HologramProjector>();
                var networkStateMachine = seer.GetComponent<NetworkStateMachine>();
                var stateMachine = seer.GetComponent<EntityStateMachine>();
                var purchaseInteraction = seer.GetComponent<PurchaseInteraction>();

                // the death state removes the model, so the sign must sit outside of it
                if (!modelLocator || !modelLocator.modelTransform || modelLocator.modelBaseTransform || modelLocator.modelTransform.parent != seer.transform
                    || !projector || !projector.hologramPivot || projector.hologramPivot.parent != seer.transform || !projector.disableHologramRotation
                    || !networkStateMachine || !stateMachine || networkStateMachine.stateMachines.Length != 1 || networkStateMachine.stateMachines[0] != stateMachine
                    || !purchaseInteraction)
                {
                    throw new InvalidOperationException("The Lunar Seer no longer has the expected price sign.");
                }
                if (EntityStateCatalog.GetStateIndex(typeof(EntityStates.Wisp1Monster.DeathState)) == EntityStateIndex.Invalid
                    || EntityStateCatalog.GetStateIndex(typeof(EntityStates.Idle)) == EntityStateIndex.Invalid)
                {
                    throw new InvalidOperationException("The states the price signs need are missing.");
                }

                seer.name = "EquipmentPriceSign";
                purchaseInteraction.Networkcost = ModConfig.EquipmentCost.Value;
                purchaseInteraction.NetworkcostType = ModConfig.EquipmentCostType.Value;

                // readable from the terminal's front, like the Seer's own sign in front of its table
                var pivot = projector.hologramPivot;
                var signPosition = terminal.transform.position + Vector3.up * SignHeight;
                var signRotation = terminal.transform.rotation * Quaternion.Euler(0f, 180f, 0f);
                var sign = new Sign { terminal = terminal, seer = seer.GetComponent<NetworkIdentity>() };
                sign.clientRotation = signRotation * Quaternion.Inverse(pivot.localRotation);
                sign.clientPosition = signPosition - sign.clientRotation * pivot.localPosition;

                NetworkServer.Spawn(seer);

                // the host keeps the Seer far away and moves only its sign; transforms are not synced
                pivot.SetPositionAndRotation(signPosition, signRotation);
                return sign;
            }
            catch
            {
                NetworkServer.Destroy(seer);
                throw;
            }
        }

        private IEnumerator SynchronizeClients()
        {
            var interval = new WaitForSecondsRealtime(1f);

            // clients are ready after the scene loads, and players can join late
            while (NetworkServer.active)
            {
                for (int i = signs.Count - 1; i >= 0; i--)
                {
                    var sign = signs[i];
                    if (!sign.seer || !CanStillBeBought(sign.terminal))
                    {
                        if (sign.seer)
                        {
                            NetworkServer.Destroy(sign.seer.gameObject);
                        }
                        signs.RemoveAt(i);
                        continue;
                    }

                    sign.synchronizedClients.RemoveWhere(connection => !connection.isReady || !NetworkServer.connections.Contains(connection));
                    foreach (var connection in NetworkServer.connections)
                    {
                        // the host already shows the sign, and on the server the death state would destroy the Seer
                        if (connection == null || !connection.isReady || Util.ConnectionIsLocal(connection) || sign.synchronizedClients.Contains(connection))
                        {
                            continue;
                        }
                        // the client needs the Seer spawned before messages can target it
                        if (sign.seer.observers == null || !sign.seer.observers.Contains(connection))
                        {
                            continue;
                        }
                        if (SendSetup(sign, connection))
                        {
                            sign.synchronizedClients.Add(connection);
                        }
                    }
                }

                if (signs.Count == 0)
                {
                    break;
                }
                yield return interval;
            }

            Destroy(gameObject);
        }

        private static bool CanStillBeBought(GameObject terminal)
        {
            if (!terminal)
            {
                return false;
            }
            if (terminal.TryGetComponent(out InstancedPurchase instance))
            {
                foreach (var player in PlayerCharacterMasterController.instances)
                {
                    if (instance.GetOrOriginal(player).available)
                    {
                        return true;
                    }
                }
                return false;
            }
            var purchaseInteraction = terminal.GetComponent<PurchaseInteraction>();
            return purchaseInteraction && purchaseInteraction.available;
        }

        // The Seer's spawn message and these share the reliable channel, so they arrive in order.
        private static bool SendSetup(Sign sign, NetworkConnection connection)
        {
            var writer = new NetworkWriter();
            // Idle ends the death state before it does anything else
            WriteState(writer, sign.seer.netId, new EntityStates.Wisp1Monster.DeathState());
            WriteState(writer, sign.seer.netId, new EntityStates.Idle());

            var teleportWriter = new NetworkWriter();
            teleportWriter.StartMessage(UmsgType.Teleport);
            new TeleportHelper.TeleportMessage
            {
                gameObject = sign.seer.gameObject,
                newPosition = sign.clientPosition,
                newRotation = sign.clientRotation,
                useRotation = true,
                delta = sign.clientPosition - SetupPosition
            }.Serialize(teleportWriter);
            teleportWriter.FinishMessage();
            var teleportBytes = teleportWriter.ToArray();
            writer.Write(teleportBytes, teleportBytes.Length);

            var messages = writer.ToArray();
            return connection.SendBytes(messages, messages.Length, QosChannelIndex.defaultReliable.intVal);
        }

        private static void WriteState(NetworkWriter writer, NetworkInstanceId seerId, EntityStates.EntityState state)
        {
            // StartMessage resets a writer, so each message gets its own
            var stateWriter = new NetworkWriter();
            stateWriter.StartMessage(UmsgType.SetEntityState);
            stateWriter.Write(seerId);
            stateWriter.Write((byte)0);
            stateWriter.Write(EntityStateCatalog.GetStateIndex(state.GetType()));
            state.OnSerialize(stateWriter);
            stateWriter.FinishMessage();
            var stateBytes = stateWriter.ToArray();
            writer.Write(stateBytes, stateBytes.Length);
        }
    }
}
