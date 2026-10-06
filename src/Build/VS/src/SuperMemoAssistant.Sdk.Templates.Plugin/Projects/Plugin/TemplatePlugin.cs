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
// 
// 
// Created On:   $time$
// Modified By:  $username$

#endregion




namespace $finalSafeProjectName$
{
  using System.Diagnostics.CodeAnalysis;
  using SuperMemoAssistant.Services.Sentry;

  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
  public class $finalPluginName$Plugin : SentrySMAPluginBase<$finalPluginName$Plugin>
  {
    #region Constructors

    /// <inheritdoc />
    public $finalPluginName$Plugin() : base("$sentryDsn$") { }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "$finalPluginName$";

    /// <inheritdoc />
    public override bool HasSettings => false;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    protected override void OnSMStarted(bool wasSMAlreadyStarted)
    {
      // Insert code that needs to be run when the plugin is initialized.
      // Typical initialization code consists of:
      // - Registering keyboard hotkeys
      // - Registering to be notified about events (e.g. OnElementChanged)
      // - Initializing your own services
      // - Publishing services for other plugins

      // If you have questions or issues, you can:
      // - Check our wiki for developer guides https://sma.supermemo.wiki/
      // - Browse through our plugins' source code https://github.com/supermemo/
      // - Ask for help on our Discord server https://discord.gg/vUQhqCT

      // Uncomment to register an event handler which will be notified when the displayed element changes
      // Svc.SM.UI.ElementWdw.OnElementChanged += new ActionProxy<SMDisplayedElementChangedEventArgs>(OnElementChanged);

      base.OnSMStarted(wasSMAlreadyStarted);
    }
    
    // Set HasSettings to true, and uncomment this method to add your custom logic for settings
    // /// <inheritdoc />
    // public override void ShowSettings()
    // {
    // }

    #endregion




    #region Methods
    
    // Uncomment to register an event handler for element changed events
    // [LogToErrorOnException]
    // public void OnElementChanged(SMDisplayedElementChangedEventArgs e)
    // {
    //   try
    //   {
    //     Insert your logic here
    //   }
    //   catch (RemotingException) { }
    // }

    #endregion
  }
}
