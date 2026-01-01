import './style.css';
import { createOrder, confirmPayment, cancelOrder } from './api.js';
import { setupOrderSocket } from './socket.js';

// UI Element Selectors
const payBtn = document.getElementById('payBtn');
const msg = document.getElementById('msg');
const emailInput = document.getElementById('email');
const amountInput = document.getElementById('amount');

// Environment Variables
const RZP_KEY = import.meta.env.VITE_RAZORPAY_KEY_ID;

/**
 * Main Payment Workflow
 */
payBtn.onclick = async function () {
    const email = emailInput.value;
    const amount = amountInput.value;

    // Basic Validation
    if (!email || !amount) {
        updateStatus("Please enter email and amount.", "red");
        return;
    }

    // Initial UI State
    payBtn.disabled = true;
    updateStatus("Creating your order...", "blue");

    let socketConnection = null;

    try {
        // STEP 1: Create the internal order in Order.Service
        const { orderId } = await createOrder(email, amount);
        console.log("Order Created Success:", orderId);

        // STEP 2: Initialize SignalR and join the specific order group
        updateStatus("Waiting for payment gateway...", "orange");
        
        socketConnection = await setupOrderSocket(orderId, (razorpayId) => {
            // This callback runs once the Payment Service publishes the Razorpay Order ID
            handleRazorpayFlow(razorpayId, orderId, amount, socketConnection);
        });

    } catch (err) {
        console.error("Workflow Error:", err);
        updateStatus(`Error: ${err.message}`, "red");
        payBtn.disabled = false;
    }
};

/**
 * Handles the Razorpay Popup and Verification
 */
function handleRazorpayFlow(razorpayOrderId, internalOrderId, amount, connection) {
    updateStatus("Opening Secure Gateway...", "blue");

    const options = {
        "key": RZP_KEY,
        "amount": amount * 100, // Amount in paise
        "currency": "INR",
        "name": "E-Commerce System",
        "description": "Microservices Task Integration",
        "order_id": razorpayOrderId,
        "handler": async function (response) {
            // STEP 3: Decoupled Verification - Call Payment.Service directly
            updateStatus("Verifying payment signature...", "orange");
            
            try {
                const verifyRes = await confirmPayment({
                    orderId: internalOrderId,
                    razorpayOrderId: response.razorpay_order_id,
                    razorpayPaymentId: response.razorpay_payment_id,
                    razorpaySignature: response.razorpay_signature
                });

                if (verifyRes.ok) {
                    updateStatus("✅ Payment Verified!", "green");
                    // Redirect to success page
                    setTimeout(() => {
                        window.location.href = `/success.html?orderId=${internalOrderId}`;
                    }, 1500);
                } else {
                    const error = await verifyRes.json();
                    throw new Error(error.message || "Signature Verification Failed");
                }
            } catch (vErr) {
                updateStatus(`Verification Failed: ${vErr.message}`, "red");
                payBtn.disabled = false;
            }
        },
        "modal": {
            "ondismiss": async function () {
                updateStatus("Payment cancelled by user.", "black");
                payBtn.disabled = false;
                // Notify Order.Service to clean up
                await cancelOrder(internalOrderId);
            }
        },
        "theme": { "color": "#3498db" }
    };

    const rzp = new window.Razorpay(options);
    rzp.open();
    
    // Once modal is open, we can close the socket connection
    if (connection) connection.stop();
}

/**
 * Utility to update the UI message
 */
function updateStatus(text, color) {
    msg.innerText = text;
    msg.style.color = color;
}