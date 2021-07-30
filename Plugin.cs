using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Actors.Types.NonPlayer;
using Dalamud.Game.Command;
using Dalamud.Plugin;

namespace Reset_dummy_enmity_command
{
    public class Plugin:IDalamudPlugin
    {
	    private DalamudPluginInterface pi;
	    public void Dispose()
	    {
		    pi.CommandManager.RemoveHandler("/resetenmity");
			pi.Dispose();
	    }

	    public void Initialize(DalamudPluginInterface pluginInterface)
	    {
		    pi = pluginInterface;
		    IntPtr ptr = pi.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 8D 43 0A");
		    int offset = Marshal.ReadInt32(pi.TargetModuleScanner.ScanText("88 88 ?? ?? ?? ?? 48 8B 4B 08") + 2);
		    PluginLog.Verbose($"ExecuteCommand: {ptr.ToInt64():X} offset: +{offset:X}");
		    pi.CommandManager.AddHandler("/resetenmity", new CommandInfo((command, arguments) =>
		    {
			    unsafe
			    {
				    var ExecuteCommand = (delegate*<int, int, int, int, int, long>)ptr;
				    var target = pi.ClientState.Targets.CurrentTarget;
				    if (target is BattleNpc { NameId: 541 } dummy && (uint)dummy.ActorId != 0xE000_0000)
				    {
					    if (Marshal.ReadByte(target.Address + offset) != 0)
					    {
						    ExecuteCommand(0x140, dummy.ActorId, 0, 0, 0);
						    PluginLog.Information($"clear target dummy enmity {dummy.ActorId:X}");
					    }
				    }
			    }
		    }){ HelpMessage = "Reset target striking dummy enmity."});
	    }

	    public string Name => "Reset dummy enmity command";
    }
}
