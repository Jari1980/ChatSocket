# Chat
<p>This is my first approach on websockets so I build a Chat program with help of ai where any number of user can chat with eact other.</p>
<img width="1035" height="545" alt="image" src="https://github.com/user-attachments/assets/afbe1ed3-f887-4086-8999-55ebb1f25200" />
<p>The application runs locally as is now, if you run this be sure that client is listening to correct port, it is set to 5140 here but local settings could be different, line12 in App.jsx.</p> 
<img width="582" height="186" alt="image" src="https://github.com/user-attachments/assets/7a206012-4e9f-474c-b331-ed050eae2cf8" />
<p>I had this running in Azure tested and worked fine. If you want to run this over internet then the backend part should be deployed to as a web app in order to run the chat server, then moving to client you need to update the ws.current in App.jsx to proper adress instead of local then you need to create a build file by running "npm run build" in console in order to create a build folder which should be deployed as a static site. The adress should start with "wss" instead of "ws", also make sure that websockets are anebled for the server when running over internet.</p>
<br/>
<br/>
<p>Happy Coding!</p>
