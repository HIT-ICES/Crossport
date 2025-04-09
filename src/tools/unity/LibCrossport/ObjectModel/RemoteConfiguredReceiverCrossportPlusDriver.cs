using System;
using System.Threading;
using Assets.Scripts.LibCrossport.Settings;
using CrossportPlus;
using CrossportPlus.ComponentDemo;
using Unity.RenderStreaming;
using UnityEngine;

namespace Ices.Crossport.ObjectModel
{
    public class RemoteConfiguredReceiverCrossportPlusDriver : CrossportDriverBase
    {
        [SerializeField] private AudioStreamReceiver gameAudio;
        [SerializeField] private VideoStreamReceiver mainCamera;
        [SerializeField] private AudioSource targetAudio;
        [SerializeField] private SingleConnection connection;
        [SerializeField] private FinalCamera finalCamera;
        // [SerializeField] private ReceiverCamera receiverCamera;
        public event Action<RemoteConfiguredReceiverCrossportPlusDriver>? OnStart;
        public event Action<RemoteConfiguredReceiverCrossportPlusDriver>? OnStop;

        public Texture recvTexture;
        // private RenderTexture skyboxTexture;
        // public RenderTexture debugTexture;

        protected override void ConfigureSignalling(CrossportSignaling signaling)
        {
            signaling.OnStart += signalingOnStart;
            signaling.OnDestroyConnection += signalingOnDestroyConnection;

            return;


            void signalingOnDestroyConnection(Unity.RenderStreaming.Signaling.ISignaling signaling, string connectionId)
            {
                OnStop?.Invoke(this);
                Thread.Sleep(1000);
                connection.CreateConnection(Guid.NewGuid().ToString("N"));
            }

            void signalingOnStart(Unity.RenderStreaming.Signaling.ISignaling signaling)
            {
                connection.CreateConnection(Guid.NewGuid().ToString("N"));
                gameAudio.targetAudioSource = targetAudio;
                OnStart?.Invoke(this);
            }
        }

        protected override void Configure(CrossportSetting config)
        {
            config.Video
                    (nameof(mainCamera))
                .Configure(mainCamera);
            config.Audio(nameof(gameAudio)).Configure(gameAudio);
            mainCamera.OnUpdateReceiveTexture += OnUpdateReceiveTexture;
            
            gameAudio.OnUpdateReceiveAudioSource += source =>
            {
                source.loop = true;
                source.Play();
            };
        }

        private void OnUpdateReceiveTexture(Texture texture)
        {
            if (texture == null)
            {
                return;
            }
            Debug.Log($"Texture Received. size={texture.width}x{texture.height}");
            // receiverCamera.skyboxTexture = texture;

            if (recvTexture != null)
            {
                finalCamera.componentRgbdTextures.Remove(recvTexture);
            }

            finalCamera.componentRgbdTextures.Add(texture);
            recvTexture = texture;

            // if (debugTexture != null)
            // {
            //     Graphics.Blit(texture, debugTexture);
            // }
            //
            // if (skyboxTexture == null)
            // {
            //     skyboxTexture = new RenderTexture(1024, 1024, 24)
            //     {
            //         dimension = UnityEngine.Rendering.TextureDimension.Cube
            //     };
            // }
            // // CrossportPlus.Utils.TextureUtils.Tex2DToCubeWithScale(texture, skyboxTexture);
            // // CrossportPlus.Utils.SkyboxUtils.SetCubemapToSkybox(skyboxTexture);
            // CrossportPlus.Utils.SkyboxUtils.SetTexture2DToSkybox(debugTexture);
        }
    }
}