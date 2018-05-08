# IMPORTANT

---------------------------------

<dl>
<div style="background-color:salmon;border-radius:15px;padding:20px;font-size:1.3em;font-weight:bold;">
Please note that the Solution Packager does not currently support Custom Controls.
<div>
</dl>

--------------------------------


All Custom Control Solution Files **MUST** be copied to the DeploymentPackage solution in the PkgFolder.  Then the property **Copy to Output Directory** needs to be set to **Always Copy**.

Any zip files placed directly in the PkgFolder will be commited as is, 
which is why it is important to continue to maintain a **manually 
extracted collection of custom controls in this folder**.  However, to simplify 
import and change management, **currently the plan is to make every custom 
control it's own solution until such time that solution packager properly
supports custom controls**.

In this folder, should be extracted solution file with the 
name of the control.  Each control should have a ControlManifest.xml 
as well as strings folder with the language based resx strings for 
configuration.

If you have any questions concerning this. Please feel free to contact us
for more details.