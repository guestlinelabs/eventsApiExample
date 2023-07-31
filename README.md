# Introduction 
This small project contains C# code with an example of how Guestline Events API can be used to:

- Get URLs for the latest snapshot containing all the raw events for a given site and event stream (rooms).
- Download the blob using the URL and process all the events streaming the blob content, deserializing and processing each event.

The example is a simple Console app that asks for a hotel group and site ids, gets the url for the latest rooms snapshot for that hotel and downloads all the room events to calculate the number of valid rooms.

# Note
This code is not production ready code nor has been tested. It is just an example to illustrate how the API can be used, and should be taken only as reference to build your own solutions as per your requirements.