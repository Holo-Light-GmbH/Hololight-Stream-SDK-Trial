# Config File Format

The remoting library uses a JSON-formatted file called `remoting-config.cfg` for runtime configuration. Each configuration item described hereafter is mandatory unless stated otherwise.

## Role

Key: `"role"`  
Valid values: `"client", "server"`  
Type: `"String"`

## Video Source

Key: `"video-source"`  
Valid values: `"none"`, `"d3d11"`  
Type: `"String"`

Values other than `"none"` are currently only supported when `"role"` is `"server"`.

## Encoder

Key: `"encoder"`  
Valid values: `"builtin"`, `"h264-uwp"`  
Type: `"String"`

## Decoder

Key: `"decoder"`  
Valid values: `"builtin"`, `"h264-uwp"`  
Type: `"String"`

## ICE Servers

Key: `"ice-servers"`  
Valid values: any URL, e.g. `"stun:stun.l.google.com:19302"`  
Type: `"Array"`  

*This item is optional.*

This holds the URIs of servers that are to be used for ICE (Internet Connection Establishment), which is how WebRTC connects two peers together.

## Diagnostic Options

Key: `"diagnostic-options"`  
Valid values: `"tracing"`, `"event-log"`  
Type: `"Array"`

*This item is optional.*

The `"tracing"` option can be used to enable the WebRTC internal tracing mechanism, which is useful for performance profiling. It creates JSON files that can be viewed in Chrome's tracing view.

The `"event-log"` option activates WebRTC's internal event log. This is similar to the official stats APIs, however, the generated data that is understood by the `"event_log_visualizer"` tool, which can plot the data.