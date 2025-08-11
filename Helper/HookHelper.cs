using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Text;

namespace BazaarIsMyHome
{
    public class HookHelper
    {
        public static void HookEndOfMethod<T>(ILContext il, Action<T> callback)
        {
            var methodContinue = il.DefineLabel();
            ILCursor c = new ILCursor(il);
            // go to the last ret statement
            while (c.TryGotoNext(x => x.MatchRet())) ;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(callback);
        }

        public static void HookEndOfMethod<T, U>(ILContext il, Action<T, U> callback)
        {
            var methodContinue = il.DefineLabel();
            ILCursor c = new ILCursor(il);
            // go to the last ret statement
            while (c.TryGotoNext(x => x.MatchRet())) ;
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate(callback);
        }

    }
}
