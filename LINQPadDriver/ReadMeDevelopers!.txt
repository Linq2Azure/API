To debug the LINQPad Driver:

1. In LINQPadDriver.csproj properties, click the Debug tab and for start action, choose "Start external program" and point to LINQPad.exe.
2. Make LINQPadDriver your startup project and hit F5!

Note that whenever you rebuild, the devdeploy.bat script runs which updates the driver on the local machine. Make sure LINQPad is closed
when you do this otherwise devdeploy.bat might fail due to a file being in use.

To deploy the LINQPad driver:

1. Zip up everything in the bin folder
2. Change the extension from .zip to .lpx.
