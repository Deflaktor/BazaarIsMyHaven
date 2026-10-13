using RoR2;
using RoR2.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace BazaarIsMyHaven
{
    // The Deep Void Signal's idle state turns on its beam, and leaving that state turns it off again.
    // Clients spawn the signal straight into the host's state, so the host keeps it idle and moves each client
    // into the plain Idle state with the vanilla SetEntityState message, once the signal has started there.
    public class VoidSignalBeam : MonoBehaviour
    {
        private const string BeamChildName = "IdleFX";
        // a client must have started the signal in its idle state first; a state arriving before that replaces it
        private const float ClientDelay = 2f;

        private readonly Dictionary<NetworkConnection, float> observedSince = new Dictionary<NetworkConnection, float>();
        private readonly HashSet<NetworkConnection> beamRemoved = new HashSet<NetworkConnection>();

        // Call after spawning the signal
        public static void Remove(GameObject signal)
        {
            if (NetworkServer.active && EntityStateCatalog.GetStateIndex(typeof(EntityStates.Idle)) != EntityStateIndex.Invalid)
            {
                signal.AddComponent<VoidSignalBeam>();
            }
        }

        private IEnumerator Start()
        {
            var identity = GetComponent<NetworkIdentity>();
            var networkStateMachine = GetComponent<NetworkStateMachine>();
            var interval = new WaitForSecondsRealtime(0.5f);

            // clients are ready after the scene loads, and players can join late
            while (NetworkServer.active && identity && networkStateMachine)
            {
                // the host's own view: its idle state stays, so the beam only needs to be turned off once it started
                HideLocalBeam();

                beamRemoved.RemoveWhere(connection => !connection.isReady || !NetworkServer.connections.Contains(connection));
                foreach (var connection in NetworkServer.connections)
                {
                    if (connection == null || !connection.isReady || Util.ConnectionIsLocal(connection) || beamRemoved.Contains(connection))
                    {
                        continue;
                    }
                    if (identity.observers == null || !identity.observers.Contains(connection))
                    {
                        observedSince.Remove(connection);
                        continue;
                    }
                    if (!observedSince.TryGetValue(connection, out float since))
                    {
                        observedSince[connection] = Time.unscaledTime;
                        continue;
                    }
                    if (Time.unscaledTime - since >= ClientDelay && SendIdle(identity, networkStateMachine, connection))
                    {
                        beamRemoved.Add(connection);
                    }
                }
                yield return interval;
            }
        }

        private void HideLocalBeam()
        {
            var modelLocator = GetComponent<ModelLocator>();
            var childLocator = modelLocator && modelLocator.modelTransform ? modelLocator.modelTransform.GetComponent<ChildLocator>() : null;
            var beam = childLocator ? childLocator.FindChildGameObject(BeamChildName) : null;
            if (beam && beam.activeSelf)
            {
                beam.SetActive(false);
            }
        }

        private static bool SendIdle(NetworkIdentity identity, NetworkStateMachine networkStateMachine, NetworkConnection connection)
        {
            var state = new EntityStates.Idle();
            var writer = new NetworkWriter();
            writer.StartMessage(UmsgType.SetEntityState);
            writer.Write(identity);
            // the signal's only state machine
            writer.Write((byte)0);
            writer.Write(EntityStateCatalog.GetStateIndex(state.GetType()));
            state.OnSerialize(writer);
            writer.FinishMessage();
            return connection.SendWriter(writer, networkStateMachine.GetNetworkChannel());
        }
    }
}
