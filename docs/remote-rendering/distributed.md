---
sidebar_position: 2
---

# Distributed Rendering

As told in the [Getting Started](/docs/remote-rendering/start):

>Originally, Render Streaming can transport video (screen, game camera, web camera, or texture), audio (game audio or microphone) and control data (user input or game data) bidirectionally from Unity application to another Unity application or web browser.

Thus, if we can separately integrate two or more render streaming instances into one application, we can use one as the rendering provider for clients and the others as the render consumer for other renderers, a distributed rendering system. Crossport realized such a possibility by crossport setting system ---- You can provide different crossport settings for different render streaming instances. Let's split the application in this post and start rendering distributedly.

## Decide what to split

Firstly, you need to decide the split parts of your application. Everything that can be shot in a screen, texture, or game camera can be split into one renderer part, e.g., a whole scene, a skybox, a complex in-scenario object, etc.

### An Example: Classroom

For example, in a classroom metaverse scene with windows, a podium, and a blackboard, you can split: The windows and the world skybox outside the classroom as part 1.
2. The blackboard as part 2.
3. The podium as well as the teacher and blackboard, is part 3.
4. The classroom scene with students, their desks and chairs as part 4, this is the primary and realtime part.

So that the final structure of the metaverse application is:

$$
Client \Longleftarrow P4(Classroom) \Leftarrow\begin{cases}
    P1(Windows)\\
    P3(Podium) \Leftarrow P2(Blackboard)
\end{cases}
$$

The original, centralized remote rendering application is now split into a four-part distributed rendering application. P1, P2, and P3 could be pre-rendered or real-time-rendered, but they can be shared between different scenes (other classrooms or other applications).

### Principals for splitting

The main advantages of the splitting are:

- Make the parts of the application that don't change much can be cached to avoid repeated rendering: E.g., P1 in the classroom example.
- Share parts of the application between different users or scenes: E.g., P3 in the classroom example.
- Split out the hard-to-render parts of the application to avoid excessive pressure on a single node. E.g., P2 in the classroom example.

Therefore, the principles for splitting an application are:

1. At least one advantage above could be taken by splitting.
2. It's possible to capture the split part in a screen, texture, or game camera.
3. Only one primary and real-time part is preserved to serve as the renderer for the final client (P4 in the classroom example).

## How to split

### Set up every rendering providers

Please set up render streaming for each part of the application following the steps in [Getting Started](/docs/remote-rendering/start). Notice: each part should have a different component name in its settings. You may need to add image transformations or scripts (e.g., panning transformations) to the camera to ensure it captures the correct scene.

### Decide where to render to

Typically, the rendering video streams are accepted as textures. So you have to set up a GameObject whose texture is the rendering target, or you can use the skybox for non-SRP applications.

The rendering audio streams can be accepted as audio sources. You can use it by adding a new audio source to the scene and then connecting it to the received audio with CrossportDriver. However, it's recommended to put the audio in the primary part.

## Configure RenderStreaming for rendering consumer

1. Assuming you have turned off **Automatic Streaming** following the [official tutorial](https://docs.unity3d.com/Packages/com.unity.renderstreaming@3.1/manual/create-scene.html) ('Changing Project Settings' section).
2. Add `SignallingManager` Component to a GameObject, and uncheck `Run On` Awake`.
3. Add `SingleConnection` Component to a GameObject, and add it to the `Signaling Handler List` of `SignallingManager`.
4. Add and configure `VideoStreamReceiver` and `AudioStreamReceiver` on demand, but you must add an `InputSender` to start the streaming. Then add them to the `Streams` of the `SingleConnection` Component.
5. Assuming you have set up Input Actions following the [official tutorial](https://docs.unity3d.com/Packages/com.unity.renderstreaming@3.1/manual/control-camera.html), remember to check the settings of the Input System.

## Implement `CrossportDriver` for rendering consumer

It would help if you created a new script containing a class inheriting `CrossportDriverBase`. The script should be attached to some GameObject as a Component. It contains the following members:

|Name|Type|Description|
|---|---|---|
|**Target video/audio accepter**|**Required** SerializeField of current class|Texture or audio source accepters to accept streams, which should be 
attached to game objects or components in the unity Editor UI|
|**SingleConnection**|**Required** SerializeField of current class|The SingleConnection component created just now, which should be attached to the component in the unity Editor UI|
|Stream senders and receivers|**Required** SerializeField of current class|Render streaming video/audio senders and receivers, which should be attached to components in the unity Editor UI|
|Configure(CrossportSetting)|**Required** protected virtual method|Called after crossport settings are loaded. It's normally used to configure stream senders and receivers.|
|Configure(CrossportSignaling)|*Optional* protected virtual method|Called after crossport signaling is created. It's normally used to start render streaming SingleConnection.|
|Awake()|*Optional* public virtual MonoBehaviour method|`MonoBehaviour.Awake()` |
|SignalingManager|**Required** SerializeField of base class|A render streaming SignalingManager, which should be attached to an instance in the unity Editor UI|
|defaultConfigName|*Optional* SerializeField of base class|Crossport setting filename prefix to be used, which can be configured in the Unity Editor UI. Normally, the setting file is the `${defaultConfigName}.cpcfg.json` file in the startup directory.|
|defaultConfig|*Optional* SerializeField of base class|The default config to use if crossport setting file is not found, which can be configured in the Unity Editor UI|
|disabled|*Optional* SerializeField of base class|Disable the drive. It can be configured in the Unity Editor UI.|

Here is an example of receiver drivers:

```csharp
public class ExampleReceiver : CrossportDriverBase
{
    [SerializeField] AudioStreamReceiver gameAudio;
    [SerializeField] VideoStreamReceiver skyBox;
    [SerializeField] AudioSource targetAudio;
    [SerializeField] private SingleConnection connection;
    protected override void Configure(CrossportSignaling signaling)
    {
        signaling.OnStart += Signaling_OnStart;
        signaling.OnDestroyConnection += Signaling_OnDestroyConnection;
    }

    private void Signaling_OnDestroyConnection(Unity.RenderStreaming.Signaling.ISignaling signaling, string connectionId)
    {
        // Auto reconnect
        Thread.Sleep(1000);
        connection.CreateConnection(Guid.NewGuid().ToString("N"));
    }

    private void Signaling_OnStart(Unity.RenderStreaming.Signaling.ISignaling signaling)
    {
        connection.CreateConnection(Guid.NewGuid().ToString("N"));
        gameAudio.targetAudioSource = targetAudio;
        
    }
    protected override void Configure(CrossportSetting config)
    {
        config.Video(nameof(skyBox)).Configure(skyBox);
        config.Audio(nameof(gameAudio)).Configure(gameAudio);
    }
    private void StoreIntoTempTexture(Texture texture)
    {
        RenderSettings.skybox.mainTexture = texture;
    }
    public override void Awake()
    {
        base.Awake();
        skyBox.OnUpdateReceiveTexture = StoreIntoTempTexture;
        gameAudio.OnUpdateReceiveAudioSource += source =>
        {
            source.loop = true;
            source.Play();
        };

    }

}

```

## Write the crossport setting file for the receiver

Create a `${defaultConfigName}.cpcfg.json` file in the root directory of your project, the filename must be different from the provider setting file of the current part. Most contents are the same as the crossport setting file of the target provider that the receiver is designed to connect to, except the `signaling.capacity`, it must be set to 0 to be marked as a consumer.
