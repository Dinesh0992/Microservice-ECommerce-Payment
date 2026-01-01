const ORDER_BASE = import.meta.env.VITE_ORDER_SERVICE_URL;
const PAYMENT_BASE = import.meta.env.VITE_PAYMENT_SERVICE_URL;

export const createOrder = async (email, amount) => {
    const res = await fetch(`${ORDER_BASE}/api/Orders/CreateOrder`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ amount: parseFloat(amount), customerEmail: email })
    });
    if (!res.ok) throw new Error("Could not create order");
    return res.json();
};

export const confirmPayment = async (payload) => {
    return fetch(`${PAYMENT_BASE}/api/Payments/ConfirmPayment`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });
};

export const cancelOrder = async (orderId) => {
    try {
        await fetch(`${ORDER_BASE}/api/Orders/CancelOrder/${orderId}`, { 
            method: 'POST' 
        });
    } catch (err) {
        console.error("Cancellation notification failed:", err);
    }
};