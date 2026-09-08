import { useState } from "react";
import {
  PaymentElement,
  useStripe,
  useElements,
} from "@stripe/react-stripe-js";
import { Button } from "@/components/ui/button";
import toast from "react-hot-toast";

interface Props {
  onSuccess: () => void;
  onError: (msg: string) => void;
}

export default function StripePaymentForm({ onSuccess, onError }: Props) {
  const stripe = useStripe();
  const elements = useElements();
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!stripe || !elements) {
      toast.error("Stripe not loaded. Check your publishable key.");
      return;
    }

    setLoading(true);
    try {
      const { error } = await stripe.confirmPayment({
        elements,
        confirmParams: {
          return_url: window.location.origin,
        },
        redirect: "if_required",
      });

      if (error) {
        onError(error.message || "Payment failed");
        toast.error(error.message || "Payment failed");
      } else {
        toast.success("Payment successful!");
        onSuccess();
      }
    } catch (err: any) {
      onError(err.message || "Payment error");
      toast.error(err.message || "Payment error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <PaymentElement options={{ layout: "tabs" }} />
      <Button
        type="submit"
        disabled={!stripe || loading}
        className="w-full h-[50px] bg-[#1a1a1a] hover:bg-[#333] text-white text-sm font-medium rounded-lg"
      >
        {loading ? "Processing..." : "Pay now"}
      </Button>
    </form>
  );
}
