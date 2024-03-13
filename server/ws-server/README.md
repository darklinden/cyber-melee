# Websocket generic lockstep server demo

## About The Project

To make a generic lockstep server demo based on netease-pomelo protocol structure and actix Actor system.

## Built With

* [Actix] <https://github.com/actix/actix>
 
## Workflow

* [x] user connect to server
* [x] user enqueue to waiting queue
* [x] if there are enough users in waiting queue, match them and create a room
* [x] user dequeue from waiting queue and enter to room
* [x] user loading room resources and send progress to server
* [x] room broadcast progress messages to all users in room
* [x] when all loading is done, room broadcast start message to all users in room
* [x] client push action to server, server save the action to room
* [x] server broadcast action to all users in room by frame
* [ ] user dequeue from room and enqueue to waiting queue

## Known Issues

* If user disconnect from server in match, the user will be marked as failed.
