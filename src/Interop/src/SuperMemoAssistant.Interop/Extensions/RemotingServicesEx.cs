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




// ReSharper disable IdentifierTypo

namespace SuperMemoAssistant.Extensions
{
  using System;
  using System.Linq;
  using System.Runtime.Remoting;
  using System.Runtime.Remoting.Channels;
  using System.Runtime.Remoting.Channels.Ipc;
  using System.Runtime.Serialization.Formatters;
  using System.Security.AccessControl;
  using System.Security.Cryptography;
  using System.Security.Principal;
  using System.Text;
  using Anotar.Serilog;

  /// <summary>Extension methods for <see cref="System.Runtime.Remoting" /></summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix",
                                                   Justification = "Extending services")]
  public static class RemotingServicesEx
  {
    #region Methods

    /// <summary>
    ///   Creates a proxy to the given service. Always return a non-null value, even if the server isn't reachable. Connection
    ///   is attempted on the first call to one of <typeparamref name="IService" /> properties or methods
    /// </summary>
    /// <typeparam name="IService"></typeparam>
    /// <param name="channelName"></param>
    /// <param name="channelPort"></param>
    /// <returns></returns>
    public static IService ConnectToIpcServer<IService>(
      string channelName,
      string channelPort = null)
    {
      return (IService)Activator.GetObject(
        typeof(IService),
        "ipc://" + channelName + "/" + (channelPort ?? channelName));
    }

    /// <summary>
    ///   Publishes the <paramref name="service" /> service with interface contract <typeparamref name="IService" />.
    /// </summary>
    /// <typeparam name="IService"></typeparam>
    /// <typeparam name="TService"></typeparam>
    /// <param name="service"></param>
    /// <param name="channelName"></param>
    /// <param name="portName"></param>
    /// <param name="aclSid"></param>
    /// <returns></returns>
    public static IpcServerChannel CreateIpcServer<IService, TService>(
      TService         service,
      string           channelName = null,
      string           portName    = null,
      WellKnownSidType aclSid      = WellKnownSidType.WorldSid)
      where IService : class
      where TService : MarshalByRefObject, IService
    {
      if (channelName == null)
        channelName = GenerateIpcServerChannelName();

      System.Collections.IDictionary properties = new System.Collections.Hashtable
      {
        { "name", channelName },
        { "portName", portName ?? channelName }
      };

      // Setup ACL : allow access from all users. Channel is protected by a random name.
      DiscretionaryAcl dacl = new DiscretionaryAcl(false, false, 1);

      dacl.AddAccess(
        AccessControlType.Allow,
        new SecurityIdentifier(
          aclSid,
          null),
        -1,
        InheritanceFlags.None,
        PropagationFlags.None);

      CommonSecurityDescriptor secDescr = new CommonSecurityDescriptor(false, false,
                                                                       ControlFlags.GroupDefaulted |
                                                                       ControlFlags.OwnerDefaulted |
                                                                       ControlFlags.DiscretionaryAclPresent,
                                                                       null, null, null,
                                                                       dacl);

      // Formatter sink
      // TODO: Custom sink
      BinaryServerFormatterSinkProvider binaryProv = new BinaryServerFormatterSinkProvider { TypeFilterLevel = TypeFilterLevel.Full };

      // Create server
      var ipcServer = new IpcServerChannel(properties, binaryProv, secDescr);

      ChannelServices.RegisterChannel(ipcServer, false);

      // Initialize with SMA interface
      RemotingServices.Marshal(service, channelName, typeof(IService));

      return ipcServer;
    }

    /// <summary>Generates a random name for the IPC server</summary>
    /// <returns></returns>
    public static string GenerateIpcServerChannelName()
    {
      using RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();

      var data    = new byte[30];
      var builder = new StringBuilder();

      rnd.GetBytes(data);

      for (int i = 0; i < 20 + data[0] % 10; i++)
      {
        var b = (byte)(data[i] % 62);

        if (b <= 9)
          builder.Append((char)('0' + b));
        else if (b >= 10 && b <= 35)
          builder.Append((char)('A' + (b - 10)));
        else if (b >= 36 && b <= 61)
          builder.Append((char)('a' + (b - 36)));
      }

      return builder.ToString();
    }

    /// <summary>
    ///   Safely raises the <paramref name="event" /> event. If a target from the invocation list throws a
    ///   <see cref="RemotingException" /> it is forcefully unsubscribed for the event.
    /// </summary>
    /// <param name="event">The event to raise</param>
    /// <param name="eventName">Friendly name for the event</param>
    /// <param name="unsubscribeDelegate">
    ///   The action to run to unsubscribe the given delegate from the
    ///   <paramref name="event" />
    /// </param>
    public static void InvokeRemote(
      this Action    @event,
      string         eventName,
      Action<Action> unsubscribeDelegate)
    {
      foreach (var handler in @event.GetInvocationList().Cast<Action>())
        try
        {
          handler();
        }
        catch (RemotingException remoteEx)
        {
          LogTo.Warning(remoteEx, "{EventName}: Remoting exception while notifying remote service - forcing unsubscribe", eventName);
          unsubscribeDelegate?.Invoke(handler);
        }
        catch (NullReferenceException)
        {
          LogTo.Warning("Null handler called for event {EventName}.", eventName);
        }
        catch (Exception ex)
        {
          LogTo.Warning(ex, "{EventName}: Exception while notifying remote service", eventName);
        }
    }

    /// <summary>
    ///   Safely raises the <paramref name="event" /> event. If a target from the invocation list throws a
    ///   <see cref="RemotingException" /> it is forcefully unsubscribed for the event.
    /// </summary>
    /// <typeparam name="TParam1"></typeparam>
    /// <param name="event">The event to raise</param>
    /// <param name="eventName">Friendly name for the event</param>
    /// <param name="p1">The event argument</param>
    /// <param name="unsubscribeDelegate">
    ///   The action to run to unsubscribe the given delegate from the
    ///   <paramref name="event" />
    /// </param>
    public static void InvokeRemote<TParam1>(
      this Action<TParam1>    @event,
      string                  eventName,
      TParam1                 p1,
      Action<Action<TParam1>> unsubscribeDelegate)
    {
      foreach (var handler in @event.GetInvocationList().Cast<Action<TParam1>>())
        try
        {
          handler(p1);
        }
        catch (RemotingException remoteEx)
        {
          LogTo.Warning(remoteEx, "{EventName}: Remoting exception while notifying remote service - forcing unsubscribe", eventName);
          unsubscribeDelegate?.Invoke(handler);
        }
        catch (NullReferenceException)
        {
          LogTo.Warning("Null handler called for event {EventName}.", eventName);
        }
        catch (Exception ex)
        {
          LogTo.Warning(ex, "{EventName}: Exception while notifying remote service", eventName);
        }
    }

    #endregion
  }
}
