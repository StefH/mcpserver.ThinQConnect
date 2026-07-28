# mcpserver.ThinQConnect
A MCP server as dotnet tool for the LG ThinQ Connect Route API and Device API.

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
- `--baseurl`
- `--stdio` (default)
- `--sse`

### Environment variables
- `THINQ_COUNTRY`
- `THINQ_PAT`
- `THINQ_BASEURL`

## Claude Desktop
```json
{
  "mcpServers": {
    "thinQConnect": {
      "command": "mcpserver.ThinQConnect",
      "args": [],
      "env": {
        "THINQ_COUNTRY": "NL",
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

---

## Sponsors

[Entity Framework Extensions](https://entityframework-extensions.net/?utm_source=StefH) and [Dapper Plus](https://dapper-plus.net/?utm_source=StefH) are major sponsors and proud to contribute to the development of **mcpserver.azuredevops.stdio**.

[![Entity Framework Extensions](https://raw.githubusercontent.com/StefH/resources/main/sponsor/entity-framework-extensions-sponsor.png)](https://entityframework-extensions.net/bulk-insert?utm_source=StefH)

[![Dapper Plus](https://raw.githubusercontent.com/StefH/resources/main/sponsor/dapper-plus-sponsor.png)](https://dapper-plus.net/bulk-insert?utm_source=StefH)