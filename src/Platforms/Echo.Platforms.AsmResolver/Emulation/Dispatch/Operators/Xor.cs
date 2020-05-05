using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Xor"/> operation code.
    /// </summary>
    public class Xor : BinaryNumericOperator
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Xor
        };

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, FValue left, FValue right) => 
            DispatchResult.InvalidProgram();

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, IntegerValue left, IntegerValue right)
        {
            left.Xor(right);
            context.ProgramState.Stack.Push(left);
            return DispatchResult.Success();
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, OValue left, OValue right) => 
            DispatchResult.InvalidProgram();
    }
}