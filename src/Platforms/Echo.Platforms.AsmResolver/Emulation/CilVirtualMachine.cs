using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values;
using Echo.Core.Code;
using Echo.Core.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Values;
using ExecutionContext = Echo.Concrete.Emulation.ExecutionContext;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides a dispatcher based implementation for a virtual machine, capable of emulating a single managed method
    /// body implemented using the CIL instruction set.
    /// </summary>
    public class CilVirtualMachine : IVirtualMachine<CilInstruction>, IServiceProvider, ICilRuntimeEnvironment
    {
        /// <inheritdoc />
        public event EventHandler<ExecutionTerminatedEventArgs> ExecutionTerminated;
     
        private readonly IDictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Creates a new instance of the <see cref="CilVirtualMachine"/>. 
        /// </summary>
        /// <param name="methodBody">The method body to emulate.</param>
        /// <param name="is32Bit">Indicates whether the virtual machine should run in 32-bit mode or in 64-bit mode.</param>
        public CilVirtualMachine(CilMethodBody methodBody, bool is32Bit)
            : this
            (
                methodBody.Owner.Module,
                new ListInstructionProvider<CilInstruction>(new CilArchitecture(methodBody), methodBody.Instructions),
                is32Bit
            )
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CilVirtualMachine"/>. 
        /// </summary>
        /// <param name="module">The module in which the CIL runs in.</param>
        /// <param name="instructions">The instructions to emulate..</param>
        /// <param name="is32Bit">Indicates whether the virtual machine should run in 32-bit mode or in 64-bit mode.</param>
        public CilVirtualMachine(ModuleDefinition module, IStaticInstructionProvider<CilInstruction> instructions, bool is32Bit)
        {
            Module = module;
            Instructions = instructions;
            Architecture = instructions.Architecture;
            
            Is32Bit = is32Bit;
            Status = VirtualMachineStatus.Idle;
            CurrentState = new CilProgramState();
            Dispatcher = new DefaultCilDispatcher();
            CliMarshaller = new DefaultCliMarshaller(this);
            MemoryAllocator = new DefaultMemoryAllocator(module, is32Bit);
            
            _services[typeof(ICilRuntimeEnvironment)] = this;
        }

        /// <inheritdoc />
        public VirtualMachineStatus Status
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public IProgramState<IConcreteValue> CurrentState
        {
            get;
        }

        /// <inheritdoc />
        public IStaticInstructionProvider<CilInstruction> Instructions
        {
            get;
        }
        
        /// <inheritdoc />
        public IInstructionSetArchitecture<CilInstruction> Architecture
        {
            get;
        }

        /// <inheritdoc />
        public bool Is32Bit
        {
            get;
        }

        /// <inheritdoc />
        public ModuleDefinition Module
        {
            get;
        }

        /// <inheritdoc />
        public ICliMarshaller CliMarshaller
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IMemoryAllocator MemoryAllocator
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the dispatcher used for the execution of instructions.
        /// </summary>
        public IVirtualMachineDispatcher<CilInstruction> Dispatcher
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ExecutionResult Execute(CancellationToken cancellationToken)
        {
            var context = new ExecutionContext(this, CurrentState, cancellationToken);

            try
            {
                Status = VirtualMachineStatus.Running;
                
                // Fetch-execute loop.
                while (!context.Exit)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Fetch.
                    var currentInstruction = Instructions.GetInstructionAtOffset(CurrentState.ProgramCounter);
                    
                    // Execute.
                    var result = Dispatcher.Execute(context, currentInstruction);
                    
                    if (result.IsSuccess)
                    {
                        context.Exit = result.HasTerminated;
                    }
                    else
                    {
                        // Handle exceptions thrown by the instruction. 
                        // TODO: process exception handlers.

                        // Note: We don't throw the user-code exception to conform with spec of the virtual machine
                        // interface.

                        context.Exit = true;
                        context.Result.Exception = result.Exception;
                    }
                }
            }
            finally
            {
                Status = VirtualMachineStatus.Idle;
                OnExecutionTerminated(new ExecutionTerminatedEventArgs(context.Result));
            }

            return context.Result;
        }

        /// <summary>
        /// Invoked when the execution of the virtual machine is terminated.
        /// </summary>
        /// <param name="e">The arguments describing the event.</param>
        protected virtual void OnExecutionTerminated(ExecutionTerminatedEventArgs e) => 
            ExecutionTerminated?.Invoke(this, e);

        object IServiceProvider.GetService(Type serviceType) => 
            _services[serviceType];

        /// <inheritdoc />
        public void Dispose()
        {
            MemoryAllocator?.Dispose();
        }
    }
}