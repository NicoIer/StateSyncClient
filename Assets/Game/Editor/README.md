

# MessagePack AOT Generation

```shell
export PATH="$PATH:/Users/chenjun/.dotnet/tools"

// Simple Sample:
dotnet mpc -i "GameCore.csproj" -o "MessagePackGenerated.cs"

// Use force map simulate DynamicContractlessObjectResolver
#dotnet mpc -i "GameCore.csproj" -o "MessagePackGenerated.cs" -m
```