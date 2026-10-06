#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#endregion




namespace SuperMemoAssistant.Interop.SMA
{
  using System;
  using System.Collections.Generic;
  using Notifications;
  using SuperMemo;
  using SuperMemo.Core;

  /// <summary>SuperMemo Assistant service</summary>
  public interface ISuperMemoAssistant
  {
    /// <summary>The SuperMemo service</summary>
    ISuperMemo SM { get; }

    /// <summary>Available layout names</summary>
    IEnumerable<string> Layouts { get; }

    /// <summary>The desktop notification manager</summary>
    INotificationManager NotificationMgr { get; }

    /// <summary>Triggered when the collection to be loaded in SM has been selected.</summary>
    event Action<SMCollection> OnCollectionSelectedEvent;

    /// <summary>Triggered when the SM process is created.</summary>
    event Action OnSMStartingEvent;

    /// <summary>Triggered when the SM process is fully started, and the collection loaded.</summary>
    event Action OnSMStartedEvent;

    /// <summary>
    ///   Triggered when the SM process has been stopped. Make sure to provide a visual feedback for long-running tasks.
    /// </summary>
    /// <remarks>
    ///   Warning: While SMA only allows a single instance of its executable to be run, the user can open the collection that
    ///   was just closed by running the SuperMemo executable directly.
    /// </remarks>
    event Action OnSMStoppedEvent;
  }
}
