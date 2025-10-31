import { useEffect, useRef, useState } from "react";
import "./App.css";

function App() {
    const [messages, setMessages] = useState([]);
    const [text, setText] = useState("");
    const [username, setUsername] = useState("");
    const [connected, setConnected] = useState(false);
    const ws = useRef(null);

    useEffect(() => {
        ws.current = new WebSocket("ws://localhost:5140/ws");
        //ws.current = new WebSocket("wss://chatserver-xx.azurewebsites.net/ws"); //Removed this resource from azure but keeping for further reference

        ws.current.onopen = () => {
            console.log("Connected to server");
        };

        ws.current.onmessage = (e) => {
            const msg = JSON.parse(e.data);
            if (msg.type === "system") {
                setMessages((prev) => [...prev, { type: "system", text: msg.text }]);
            } else if (msg.type === "chat") {
                setMessages((prev) => [...prev, msg]);
            }
        };

        ws.current.onclose = () => {
            setMessages((prev) => [...prev, { type: "system", text: "❌ Disconnected" }]);
        };

        return () => ws.current?.close();
    }, []);

    const handleJoin = () => {
        if (username.trim()) {
            ws.current.send(username.trim());
            setConnected(true);
        }
    };

    const sendMessage = () => {
        if (text.trim()) {
            ws.current.send(text);
            setText("");
        }
    };

    return (
        <div className="chat-container">
            {!connected ? (
                <div className="login">
                    <h1 className="broccoli-title">Broccoli goes Bananas with AI Chat</h1>
                    <h2>Enter your name</h2>
                    <input
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        placeholder="Your name"
                        onKeyDown={(e) => e.key === "Enter" && handleJoin()}
                    />
                    <button onClick={handleJoin}>Join Chat</button>
                </div>
            ) : (
                <>
                    <h1 className="broccoli-title">Chat with others</h1>
                    <div className="chat-box">
                        {messages.map((m, i) => (
                            <div key={i} className={m.type === "system" ? "system-msg" : "chat-msg"}>
                                {m.type === "chat"
                                    ? `[${m.time}] ${m.user}: ${m.text}`
                                    : m.text}
                            </div>
                        ))}
                    </div>
                    <div className="input-area">
                        <input
                            value={text}
                            onChange={(e) => setText(e.target.value)}
                            onKeyDown={(e) => e.key === "Enter" && sendMessage()}
                            placeholder="Type a message..."
                        />
                        <button onClick={sendMessage}>Send</button>
                    </div>
                </>
            )}
        </div>
    );
}

export default App;
