using RoR2;
using RoR2.Hologram;
using RoR2.Networking;
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
    // Price signs for interactables that have none for clients, like Equipment Terminals, whose price display belongs
    // to the multishop around them in vanilla, or the Donation Altar.
    // Each one gets a Lunar Seer, whose price sign is not part of its model. Clients are set up with vanilla
    // messages only: a Wisp death state removes the Seer's model and collider, and a teleport moves the sign above it.
    public class PriceSigns : MonoBehaviour
    {
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
            if (!seerPrefab.IsValid())
            {
                seerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/SeerStation.prefab");
            }
        }

        // Shows cost in costType this high above each of the terminals, readable from their front, like the Seer's own sign in front of its table
        public static void Apply(List<GameObject> terminals, int cost, CostTypeIndex costType, float signHeight)
        {
            Apply(terminals.Select(terminal => (terminal, terminal.transform.position + Vector3.up * signHeight, terminal.transform.rotation * Quaternion.Euler(0f, 180f, 0f))).ToList(), cost, costType);
        }

        // Shows cost in costType at the given place; the sign goes away once the terminal can no longer be bought
        public static void Apply(GameObject terminal, Vector3 signPosition, Quaternion signRotation, int cost, CostTypeIndex costType)
        {
            Apply(new List<(GameObject, Vector3, Quaternion)> { (terminal, signPosition, signRotation) }, cost, costType);
        }

        private static void Apply(List<(GameObject terminal, Vector3 position, Quaternion rotation)> placements, int cost, CostTypeIndex costType)
        {
            if (!NetworkServer.active || placements.Count == 0 || cost <= 0 || signsUnavailable)
            {
                return;
            }

            var syncObject = new GameObject("PriceSigns");
            SceneManager.MoveGameObjectToScene(syncObject, placements[0].terminal.scene);
            var priceSigns = syncObject.AddComponent<PriceSigns>();
            try
            {
                foreach (var (terminal, position, rotation) in placements)
                {
                    priceSigns.signs.Add(CreateSign(terminal, position, rotation, cost, costType));
                }
            }
            catch (Exception exception)
            {
                signsUnavailable = true;
                Log.LogError($"Could not create price signs: {exception}");
                foreach (var sign in priceSigns.signs)
                {
                    NetworkServer.Destroy(sign.seer.gameObject);
                }
                Destroy(syncObject);
                return;
            }
            priceSigns.StartCoroutine(priceSigns.SynchronizeClients());
        }

        private static Sign CreateSign(GameObject terminal, Vector3 signPosition, Quaternion signRotation, int cost, CostTypeIndex costType)
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

                seer.name = "PriceSign";
                purchaseInteraction.Networkcost = cost;
                purchaseInteraction.NetworkcostType = costType;

                var pivot = projector.hologramPivot;
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
            if (terminal.TryGetComponent(out GenericInteraction genericInteraction))
            {
                return genericInteraction.interactability != Interactability.Disabled;
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
