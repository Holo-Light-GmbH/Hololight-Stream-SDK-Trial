# Data Channels (Extensions)

Data channels can be created by developers to extend the default Hololight Stream functionality by sending and receiving custom data. These channels can be used as extensions to Hololight Stream, providing additional user specific functionality.

> :warning: Data channels require implementation on both client and server applications. Extensions written with data channels will only be possible if writing a custom client to send/receive the custom data.

## Registering a Data Channel

Data channels can be registered by exposing a function with the `DataChannelRegistration` attribute. This function will be called by Hololight Stream and it is expected that the implementor register themselves at this point. The below snippet provides and example of registering a data channel.

```
[DataChannelRegistration]
public static void RegisterDataChannel()
{
    DataChannelManager.RegisterDataChannel(new DataChannelDescription(
        typeof(SampleExtension),
        "com.hololight.sample",
        new IsarVersion(1,0,0),
        ChannelPriority.Low,
        false,
        false));
}
```

The name field is used for handling negotiation of channels with the client endpoint. The name should:
- Follow `<domain-name>.<company-name>.<extension-name>`, e.g. com.hololight.sample_extension
- Further naming sections should append another period
- Only contain periods (.) and underscores (_).

## DataChannel Base Class

An abstract base class, `DataChannel`, has been provided to be used as a base for all data channel extensions. This must be inherited from when implementing an extension or the registration of the channel will be rejected.

```
using namespace Hololight.Stream
public class SampleExtension : DataChannel
{
    /// Implementing code here
}
```
The DataChannel has a number of functions provided for usage/implementation:
- `Start` will open the channel, ready for sending and receiving data. If negotiation has not yet been carried out, this will be cached and called when supportability is confirmed.
- `OnSupportedChanged` will be called when the negotiation has confirmed whether the channel is supported.
- `OnConnectionChanged` will be called if the channel is supported and it was previously opened.
- `OnDataReceived` is an abstract function which must be implemented by the derived class. This call will provide the extension with data received from the client endpoint to be parsed and handled.
- `SendData` allows the implementor to send a byte array to the client endpoint.
- `Stop` will close the channel, if previously opened, and stop the client (and server) from sending any further data.

### DataChannelManager

The `DataChannelManager` is a static class which is used by Hololight Stream for instantiating and managing data channels. It is intended for implementors not to instantiate channels themselves, and instead call to the manager to grab the channel.

The `DataChannelManager.GetDataChannel` static generic function is provided for implementors to grab the instantiated data channel extension for sending and receiving data.

```
using namespace HoloLight.Isar
public class Sample : MonoBehaviour
{
    SampleExtension _sampleExtension;
    private void OnEnable()
    {
        _sampleExtension = DataChannelManager.GetDataChannel<SampleExtension>();
        if(_sampleExtension != null)
        {
            _sampleExtension.Start();

            // Register to any events
    }

    private void Update()
    {
        // Send data using the extension
    }

    private void OnDisable()
    {
        if(_sampleExtesnion != null)
        {
            _sampleExtension.Stop();
        }
    }
}
```

## Samples

For further information on data channel extensions, see the `MessagingExtension` sample within the `DataChannel` folder of the `com.hololight.stream.examples` package. This sample sends and receives string messages to and from the client for logging.

> **_NOTE:_** This sample is intended as an example of how to implement an extension. It is not functional and no client supports this extension.

