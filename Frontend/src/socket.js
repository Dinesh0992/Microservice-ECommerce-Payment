import * as signalR from '@microsoft/signalr';

/**
 * Creates and starts a SignalR connection to the Order Hub
 * @param {string} orderId - The GUID of the order to track
 * @param {function} onRazorpayIdReceived - Callback when the backend sends the Razorpay Order ID
 * @returns {Promise<signalR.HubConnection>}
 */
export const setupOrderSocket = async (orderId, onRazorpayIdReceived) => {
    const hubUrl = import.meta.env.VITE_ORDER_HUB_URL;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl)
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // Set up listener for the specific event from Order Service
    connection.on("UpdateRazorpayId", (razorpayId) => {
        console.log(`[Socket] Received Razorpay ID for Order ${orderId}:`, razorpayId);
        onRazorpayIdReceived(razorpayId);
    });

    try {
        await connection.start();
        console.log("[Socket] Connected to Order Hub");

        // Join the group specifically for this Order ID to avoid receiving other people's updates
        await connection.invoke("JoinOrderGroup", orderId.toString());
        console.log(`[Socket] Joined Group: ${orderId}`);
        
        return connection;
    } catch (err) {
        console.error("[Socket] Connection failed: ", err);
        throw err;
    }
};