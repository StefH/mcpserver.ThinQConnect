# ![Project Icon](./resources/icon_32x32.png) mcpserver.ThinQConnect
A hybrid MCP server as dotnet tool for the LG ThinQ Connect Route API and Device API.

## 📦 NuGet
[![NuGet Badge](https://img.shields.io/nuget/v/mcpserver.ThinQConnect)](https://www.nuget.org/packages/mcpserver.ThinQConnect)

## `dnx`
```cmd
dnx mcpserver.ThinQConnect --yes
```

## `dotnet tool`
### Installation
```cmd
dotnet tool install --global mcpserver.ThinQConnect
```

## 🛠️ Supported Tools
- `GetRoute`: Resolve the ThinQ backend endpoints for the configured country.
- `GetDevices`: List ThinQ devices available for the configured PAT.
- `GetDeviceProfile`: Retrieve the profile JSON for a specific ThinQ device.
- `GetDeviceState`: Retrieve the current state JSON for a specific ThinQ device.
- `ControlDevice`: Send a control payload JSON document to a specific ThinQ device.

## ⚙️ Configuration
The server reads configuration from either command-line arguments or environment variables.

### Command-line arguments
- `--country`
- `--pat`
- `--servicePhase` (optional, defaults to `OP`)
- `--clientId` (optional)
- `--baseUrl` (optional override; otherwise the server resolves the backend with `GetRoute`)
- `--stdio`
- `--sse`

### Environment variables
- `THINQ_COUNTRY`
- `THINQ_PAT`
- `THINQ_SERVICE_PHASE`
- `THINQ_CLIENT_ID`
- `THINQ_BASE_URL`

## Claude Desktop
```json
{
  "mcpServers": {
    "thinQConnect": {
      "command": "mcpserver.ThinQConnect",
      "args": [],
      "env": {
        "THINQ_COUNTRY": "US",
        "THINQ_PAT": "your-thinq-pat"
      }
    }
  }
}
```

## Notes
- The server uses the documented ThinQ Route API to resolve the correct backend for Device API calls.
- ThinQ documents a fixed `x-api-key` for these APIs; this server uses that value by default.
- `ControlDevice` accepts raw JSON so it can pass through the device-specific control payload described by LG's device profiles.
