# Basic-Cloud Companion
App to allow communication with the basic-cloud API server. Both the GUI and CLI projects will be included in this repo. It also contains shared projects that can be used to help create a new/different interface.

## Aims
- GUI interface (built with GTK)
- CLI interface

## Testing
This repo contains a testing project to allow running unit tests on certain methods. To run most of these tests you will need some 'TestRunParameters' and a running basic-cloud API server with a created user.

### Example .runsettings
```xml
<RunSettings>
    <!-- Parameters used by tests at runtime -->
    <TestRunParameters>
    <Parameter name="TEST_API_URL" value="http://localhost:8000" />
    <Parameter name="TEST_USERNAME" value="test" />
    <Parameter name="TEST_PASSWORD" value="test" />
    <Parameter name="TEST_TOKEN" value="<the secret token for test user here>" />
    </TestRunParameters>
</RunSettings>
```

Running the test can be done with `dotnet test -s <name>.runsettings`.
