﻿/// Written by: Yulia Danilova
/// Creation Date: 24th of October, 2019
/// Purpose: Implementation of IAsyncCommand and generic IAsyncCommand interfaces
/// Remarks: The two versions are pretty similar and it is tempting to only keep the latter. We could use a AsyncCommand<object> with null parameter to replace the first one. 
///          While it technically works, it is better to keep the two of them both, in the sense that having no parameter is not semantically similar to taking a null parameter.
#region ========================================================================= USING =====================================================================================
using System;
using System.Windows.Input;
using System.Threading.Tasks;
using Devonia.ViewModels.Common.Extensions;
#endregion

namespace Devonia.ViewModels.Common.MVVM
{
    public class AsyncCommand : IAsyncCommand
    {
        #region ============================================================== FIELD MEMBERS ================================================================================
        public event EventHandler CanExecuteChanged;

        private bool isExecuting;
        private readonly Func<Task> execute;
        private readonly Func<bool> canExecute;
        #endregion

        #region ================================================================== CTOR =====================================================================================
        /// <summary>
        /// Overload C-tor
        /// </summary>
        /// <param name="execute">The async Task method to be executed</param>
        /// <param name="canExecute">The method indicating whether the <paramref name="execute"/>can be executed</param>
        public AsyncCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }
        #endregion

        #region ================================================================= METHODS ===================================================================================
        /// <summary>
        /// Indicates whether the command can be executed
        /// </summary>
        /// <returns>True if the command can be executed; False otherwise.</returns>
        public bool CanExecute()
        {
            return !isExecuting && (canExecute?.Invoke() ?? true);
        }

        /// <summary>
        /// Executes a Task
        /// </summary>
        public async Task ExecuteAsync()
        {
            if (CanExecute())
            {
                try
                {
                    isExecuting = true;
                    await execute();
                }
                finally
                {
                    isExecuting = false;
                }
            }
            RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Executes the delegate that signals changes in permissions of execution of the command
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #region Explicit implementations
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// Also, see the parameter version <seealso cref="AsyncCommand{T}.CanExecute(T)">AsyncCommand{T}.CanExecute(T)</seealso></param>
        /// <returns>True if this command can be executed; otherwise, False.</returns>
        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// Also, see the parameter version <seealso cref="AsyncCommand{T}.CanExecute(T)">AsyncCommand{T}.CanExecute(T)</seealso></param>
        void ICommand.Execute(object parameter)
        {
            ExecuteAsync().FireAndForgetSafeAsync();
        }
        #endregion
        #endregion
    }

    public class AsyncCommand<T> : IAsyncCommand<T>
    {
        #region ============================================================== FIELD MEMBERS ================================================================================
        public event EventHandler CanExecuteChanged;

        private bool isExecuting;
        private readonly Func<T, Task> execute;
        private readonly Func<T, bool> canExecute;
        #endregion

        #region ================================================================== CTOR =====================================================================================
        /// <summary>
        /// Overload C-tor
        /// </summary>
        /// <param name="execute">The async Task method to be executed</param>
        /// <param name="canExecute">The method indicating whether the <paramref name="execute"/>can be executed</param>
        public AsyncCommand(Func<T, Task> execute, Func<T, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }
        #endregion

        #region ================================================================= METHODS ===================================================================================
        /// <summary>
        /// Indicates whether the command can be executed
        /// </summary>
        /// <param name="parameter">Generic parameter passed to the command to be executed</param>
        /// <returns>True if the command can be executed; False otherwise.</returns>
        public bool CanExecute(T parameter)
        {
            return !isExecuting && (canExecute?.Invoke(parameter) ?? true);
        }

        /// <summary>
        /// Executes a Task
        /// </summary>
        /// <param name="parameter">Generic parameter passed to the command to be executed</param>
        public async Task ExecuteAsync(T parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    isExecuting = true;
                    await execute(parameter);
                }
                finally
                {
                    isExecuting = false;
                }
            }
            RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Executes the delegate that signals changes in permissions of execution of the command
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #region Explicit implementations
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// Also, see the parameterless version <seealso cref="AsyncCommand.CanExecute">AsyncCommand.CanExecute</seealso></param>
        /// <returns>True if this command can be executed; otherwise, False.</returns>
        bool ICommand.CanExecute(object parameter)
        {
            // WPF bug - due to virtualization, sometimes _parameter can be automatically set to "DisconnectedItem", throwing exception
            return parameter == null || parameter.ToString() == "{DisconnectedItem}" || CanExecute((T)parameter);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// Also, see the parameterless version <seealso cref="AsyncCommand.CanExecute">AsyncCommand.CanExecute</seealso></param>
        void ICommand.Execute(object parameter)
        {
            ExecuteAsync((T)parameter).FireAndForgetSafeAsync();
        }
        #endregion
        #endregion
    }
}
