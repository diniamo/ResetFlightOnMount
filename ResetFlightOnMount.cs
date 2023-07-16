using static Mono.Cecil.Cil.OpCodes;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ResetFlightOnMount
{
	public class ResetFlightOnMount : Mod
	{
        public override void Load()
        {
            Terraria.IL_Mount.SetMount += il =>
            {
                try
                {
                    var c = new ILCursor(il);
                    var endLabel = il.DefineLabel();

                    c.GotoNext(MoveType.Before,
                        i => i.Match(Ldarg_2),
                        i => i.MatchLdfld(typeof(Entity).GetField(nameof(Entity.whoAmI))),
                        i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.myPlayer))),
                        i => i.Match(Bne_Un_S)
                    );

                    c.Emit(Ldarg_0);
                    c.Emit(Mono.Cecil.Cil.OpCodes.Call, typeof(Mount).GetMethod(nameof(Mount.CanFly)));
                    c.Emit(Brfalse, endLabel);

                    c.Emit(Ldarg_2);
                    c.Emit(Ldarg_0);
                    c.EmitDelegate<Action<Player, Mount>>((player, mount) =>
                    {
                        if (player.wingTime != player.wingTimeMax)
                        {
                            float transferAmount = Math.Min((float)player.wingTimeMax - player.wingTime, mount._fatigueMax - mount._fatigue);
                            mount._fatigue += transferAmount;
                            player.wingTime += transferAmount;
                        }

                        if (player.rocketBoots != 0 && player.rocketTime != player.rocketTimeMax)
                        {
                            int transferAmount = Math.Min(player.rocketTimeMax - player.rocketTime, (int)(mount._fatigueMax - mount._fatigue));
                            mount._fatigue += transferAmount;
                            player.rocketTime += transferAmount;
                        }
                    });

                    c.MarkLabel(endLabel);
                } catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            };

            base.Load();
        }
    }
}