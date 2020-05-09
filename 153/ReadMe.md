# About
This is what was hosted on Athena server for the project. The PHP script has 2 main functions.
* Return a list of images as a JSON object based on their relative path and the URL
* Get the Base64 encoded image from within a JSON object and then store it in public/ directory.

If you would like to use this code on Athena server, make sure you edit the permissions. This can be done with the following code.

> chmod 755 images.php
> chmod 7555 public

That should be it!
