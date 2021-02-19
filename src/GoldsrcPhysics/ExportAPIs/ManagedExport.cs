using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UsrSoft.ManagedExport
{
    /// <summary>
    /// Construct custom delegate type using given info.
    /// Returns NOT generic delegate type that can be used by `Delegate.CreateDelegate`.
    /// </summary>
    internal static class DelegateCreator
    {
        internal static readonly Func<Type[], Type> MakeNewCustomDelegate =
            (Func<Type[], Type>)Delegate.CreateDelegate
            (
                typeof(Func<Type[], Type>),
                typeof(Expression).Assembly.GetType("System.Linq.Expressions.Compiler.DelegateHelpers")
                .GetMethod("MakeNewCustomDelegate", BindingFlags.NonPublic | BindingFlags.Static)
            );
        internal static Type NewDelegateType(Type ret, params Type[] parameters)
        {
            Type[] args = new Type[parameters.Length + 1];
            parameters.CopyTo(args, 0);
            args[args.Length - 1] = ret;
            return MakeNewCustomDelegate(args);
        }
    }
    public class ManagedExporter
    {
        #region Get API pointer

        // Hold these instances to avoid being collected by the GC
        private readonly static List<object> _keepReference = new List<object>();
        private unsafe static void* GetMethodPointer(string name)
        {
            var token = name.Split('.');
            MethodInfo methodInfo = Type.GetType(string.Join(".",token.Take(token.Length-1))).GetMethod(token[token.Length-1]);

            var argTypes = methodInfo.GetParameters().Select(x => x.ParameterType);

            // also mark this delegate type with [UnmanagedFunctionPointer(CallingConvention.StdCall)] attribute
            // edit: but default marshal calling convension is stdcall so we don't need to mark explicit
            Type delegateType = DelegateCreator.NewDelegateType(methodInfo.ReturnType, argTypes.ToArray());

            var delegateInstance = Delegate.CreateDelegate(delegateType, methodInfo);

            _keepReference.Add(delegateType);
            _keepReference.Add(delegateInstance);
            return (void*)Marshal.GetFunctionPointerForDelegate(delegateInstance);
        }
        /// <summary>
        /// Gives a pointer to a sizeof(void*) buffer and method name.
        /// Then will write the funcPtr of method given by MethodName to buffer.
        /// 
        /// NOTE: sizeof(void*) depends on platform.
        /// </summary>
        /// <param name="pointerAndName">look like "0x0000FFFF|Namespace.Type.MethodName"</param>
        /// <returns></returns>
        public unsafe static int GetFunctionPointer(string pointerAndName)
        {
            var args = pointerAndName.Trim().Split('|');
            if (args.Length > 2)
                throw new ArgumentException(nameof(pointerAndName) + ":" + pointerAndName);
            void** p = (void**)Convert.ToUInt64(args[0], 16);
            p[0] = GetMethodPointer(args[1]);
            return (int)p[0];
        }
        #endregion
    }
}
