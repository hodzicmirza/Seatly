import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { validateQrTicket } from "@/api/bookings";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";
import {
  MdQrCodeScanner,
  MdCheckCircle,
  MdError,
  MdWarning,
  MdEvent,
  MdConfirmationNumber,
  MdRefresh,
  MdLocationOn,
} from "react-icons/md";
import type { BookingResponse } from "@/types";

export default function ValidateTicketPage() {
  const [qrInput, setQrInput] = useState("");
  const [lastValidatedBooking, setLastValidatedBooking] = useState<BookingResponse | null>(null);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const mutation = useMutation({
    mutationFn: (data: string) => validateQrTicket(data),
    onSuccess: (booking) => {
      setLastValidatedBooking(booking);
      setErrorMsg(null);
      toast.success("Ticket successfully validated and checked in!");
    },
    onError: (err: any) => {
      setLastValidatedBooking(null);
      const msg = err.response?.data?.error || "Invalid QR Code or ticket verification failed.";
      setErrorMsg(msg);
      toast.error(msg);
    },
  });

  const handleValidate = (e: React.FormEvent) => {
    e.preventDefault();
    if (!qrInput.trim()) {
      toast.error("Please enter or paste QR code data / ticket ID.");
      return;
    }
    mutation.mutate(qrInput.trim());
  };

  const handleReset = () => {
    setQrInput("");
    setLastValidatedBooking(null);
    setErrorMsg(null);
  };

  return (
    <div className="max-w-2xl mx-auto space-y-6 pb-12">
      <Card className="shadow-lg border rounded-2xl overflow-hidden">
        <CardHeader className="bg-muted/30 border-b pb-6">
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="text-2xl font-bold flex items-center gap-2">
                <MdQrCodeScanner className="text-primary text-2xl" /> Ticket QR Code Validator
              </CardTitle>
              <CardDescription className="mt-1">
                Admin Portal: Scan or paste a ticket QR code / Booking ID payload to verify authenticity and check in attendees.
              </CardDescription>
            </div>
            <Badge variant="secondary" className="px-3 py-1 text-xs">
              Admin / Organizer
            </Badge>
          </div>
        </CardHeader>

        <CardContent className="pt-6 space-y-6">
          <form onSubmit={handleValidate} className="space-y-4">
            <div>
              <Label htmlFor="qrData" className="font-semibold">
                QR Code Payload or Booking Reference ID *
              </Label>
              <div className="flex gap-2 mt-1.5">
                <Input
                  id="qrData"
                  required
                  placeholder="Paste QR JSON string or Booking ID GUID..."
                  value={qrInput}
                  onChange={(e) => setQrInput(e.target.value)}
                  className="font-mono text-xs sm:text-sm"
                />
                <Button type="submit" disabled={mutation.isPending || !qrInput.trim()} className="px-6 rounded-xl">
                  {mutation.isPending ? "Validating..." : "Validate"}
                </Button>
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Accepts raw QR JSON objects, Base64 strings, or Booking GUIDs.
              </p>
            </div>
          </form>

          {/* Error / Warning Alert Banner */}
          {errorMsg && (
            <div className="p-4 rounded-xl border bg-red-50 dark:bg-red-950/40 text-red-800 dark:text-red-300 border-red-300 dark:border-red-800 flex items-start gap-3">
              {errorMsg.toLowerCase().includes("already used") ? (
                <MdWarning className="text-amber-600 dark:text-amber-400 text-2xl mt-0.5 shrink-0" />
              ) : (
                <MdError className="text-red-600 dark:text-red-400 text-2xl mt-0.5 shrink-0" />
              )}
              <div className="space-y-1">
                <h4 className="font-bold text-sm">Validation Failed</h4>
                <p className="text-xs sm:text-sm leading-relaxed">{errorMsg}</p>
              </div>
            </div>
          )}

          {/* Validated Ticket Result Card */}
          {lastValidatedBooking && (
            <div className="p-5 rounded-2xl border bg-emerald-50/80 dark:bg-emerald-950/40 border-emerald-300 dark:border-emerald-800 space-y-4">
              <div className="flex items-center justify-between border-b border-emerald-200 dark:border-emerald-800 pb-3">
                <div className="flex items-center gap-2 text-emerald-800 dark:text-emerald-300">
                  <MdCheckCircle className="text-emerald-600 dark:text-emerald-400 text-2xl" />
                  <div>
                    <h3 className="font-bold text-base">VALID TICKET — CHECKED IN!</h3>
                    <p className="text-xs text-emerald-700 dark:text-emerald-400">
                      Booking Status: <span className="font-semibold uppercase">{lastValidatedBooking.status}</span>
                    </p>
                  </div>
                </div>
                <Button variant="outline" size="sm" onClick={handleReset} className="rounded-lg text-xs">
                  <MdRefresh className="mr-1" /> Scan Next
                </Button>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs sm:text-sm text-emerald-900 dark:text-emerald-200">
                <div className="flex items-center gap-2">
                  <MdEvent className="text-emerald-600 dark:text-emerald-400 text-lg shrink-0" />
                  <div>
                    <span className="text-xs text-muted-foreground block">Event Name & Date</span>
                    <strong className="font-semibold">{lastValidatedBooking.eventName}</strong>
                    <p className="text-[11px] text-muted-foreground">
                      {new Date(lastValidatedBooking.eventDate).toLocaleString("en-GB")}
                    </p>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <MdLocationOn className="text-emerald-600 dark:text-emerald-400 text-lg shrink-0" />
                  <div>
                    <span className="text-xs text-muted-foreground block">Venue Location</span>
                    <strong className="font-semibold">{lastValidatedBooking.eventLocation}</strong>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <MdConfirmationNumber className="text-emerald-600 dark:text-emerald-400 text-lg shrink-0" />
                  <div>
                    <span className="text-xs text-muted-foreground block">Category & Reserved Seats</span>
                    <strong className="font-semibold">
                      {lastValidatedBooking.category} ({lastValidatedBooking.numberOfSeats} seat{lastValidatedBooking.numberOfSeats > 1 ? "s" : ""})
                    </strong>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <div>
                    <span className="text-xs text-muted-foreground block">Total Price Paid</span>
                    <strong className="font-bold text-primary">
                      {lastValidatedBooking.totalPrice.toFixed(2)} BAM
                    </strong>
                  </div>
                </div>
              </div>

              <div className="text-[11px] text-emerald-700 dark:text-emerald-400 pt-2 border-t border-emerald-200 dark:border-emerald-800 font-mono">
                Booking ID: {lastValidatedBooking.bookingId}
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
