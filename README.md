A server helping developers to update projects running on the remote server.

# Settings

Settings in the `appsetting.json`.

```json
{
  "port": 3000,
  "modules": [
    {
      "name": "module-name",
      "path": "C:/Users/me/path/to/project",
      "pre": ["Stop-Service", "-Name service-name"],
      "post": ["Start-Service", "-Name service-name"]
    }
  ]
}
```

The `port` means the server port.

In `modules` section, many modules configurations can be set.

# Update

By http post request to `http://host:port/api/update` with a form-data including two field `name` and `file`, the update will be executed.

In which, the `name` is the module name, the `file` is an archive file, such as zip file.

The `pre` (if exists) will be executed first, then the file will be extracted and copy to the `path`, after that the `post` command will be executed.