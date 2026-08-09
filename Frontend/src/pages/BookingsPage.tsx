import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { getMyBookings, cancelBooking } from "@/api/bookings";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { toast } from "sonner";
import {
  MdEvent,
  MdCancel,
  MdQrCode2,
  MdLocationOn,
  MdConfirmationNumber,
  MdBookmark,
  MdCheckCircle,
  MdHourglassEmpty,
  MdDoNotDisturb,
  MdDownload,
} from "react-icons/md";

function statusBadge(status: string) {
  switch (status) {
    case "Confirmed":
      return (
        <Badge className="bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border-emerald-500/30 flex items-center gap-1 font-semibold">
          <MdCheckCircle className="text-emerald-500" /> Confirmed
        </Badge>
      );
    case "Pending":
      return (
        <Badge variant="secondary" className="bg-amber-500/15 text-amber-700 dark:text-amber-300 border-amber-500/30 flex items-center gap-1 font-semibold">
          <MdHourglassEmpty className="text-amber-500 animate-spin" /> Pending
        </Badge>
      );
    case "Cancelled":
      return (
        <Badge variant="destructive" className="flex items-center gap-1 font-semibold">
          <MdDoNotDisturb /> Cancelled
        </Badge>
      );
    case "Used":
      return (
        <Badge variant="outline" className="flex items-center gap-1 font-semibold">
          Used Ticket
        </Badge>
      );
    default:
      return <Badge variant="secondary">{status}</Badge>;
  }
}

export default function BookingsPage() {
  const queryClient = useQueryClient();
  const [filter, setFilter] = useState<string>("ALL");

  const { data: bookings, isLoading, error } = useQuery({
    queryKey: ["bookings"],
    queryFn: getMyBookings,
  });

  const cancelMutation = useMutation({
    mutationFn: cancelBooking,
    onSuccess: () => {
      toast.success("Booking cancelled successfully.");
      queryClient.invalidateQueries({ queryKey: ["bookings"] });
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.error || "Cancel failed.");
    },
  });

  if (isLoading) return <p className="text-muted-foreground">Loading your bookings...</p>;
  if (error)
    return (
      <p className="text-destructive font-medium">
        Failed to load bookings. Please ensure you are logged in.
      </p>
    );

  const totalBookings = bookings?.length || 0;
  const confirmedCount = bookings?.filter((b) => b.status === "Confirmed").length || 0;
  const totalSpent = bookings?.reduce((sum, b) => (b.status !== "Cancelled" ? sum + b.totalPrice : sum), 0) || 0;

  const filteredBookings = bookings?.filter((b) => {
    if (filter === "ALL") return true;
    return b.status.toUpperCase() === filter.toUpperCase();
  });

  const downloadQrCode = (base64: string, eventName: string) => {
    const link = document.createElement("a");
    link.href = `data:image/png;base64,${base64}`;
    link.download = `Seatly_Ticket_${eventName.replace(/\s+/g, "_")}.png`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <div className="space-y-8 pb-8 max-w-5xl mx-auto">
      <div className="space-y-4">
        <h1 className="text-3xl font-extrabold tracking-tight">My Bookings & Tickets</h1>
        <p className="text-muted-foreground text-sm">
          Manage your event reservations, view digital QR tickets, and check booking statuses.
        </p>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 pt-2">
          <Card className="rounded-2xl border bg-card/60 backdrop-blur-md shadow-sm">
            <CardContent className="p-4 flex items-center gap-4">
              <div className="p-3 rounded-xl bg-primary/10 text-primary">
                <MdBookmark className="text-2xl" />
              </div>
              <div>
                <div className="text-xs text-muted-foreground font-medium">Total Bookings</div>
                <div className="text-2xl font-bold">{totalBookings}</div>
              </div>
            </CardContent>
          </Card>

          <Card className="rounded-2xl border bg-card/60 backdrop-blur-md shadow-sm">
            <CardContent className="p-4 flex items-center gap-4">
              <div className="p-3 rounded-xl bg-emerald-500/10 text-emerald-600">
                <MdCheckCircle className="text-2xl" />
              </div>
              <div>
                <div className="text-xs text-muted-foreground font-medium">Confirmed Tickets</div>
                <div className="text-2xl font-bold">{confirmedCount}</div>
              </div>
            </CardContent>
          </Card>

          <Card className="rounded-2xl border bg-card/60 backdrop-blur-md shadow-sm">
            <CardContent className="p-4 flex items-center gap-4">
              <div className="p-3 rounded-xl bg-purple-500/10 text-purple-600">
                <MdConfirmationNumber className="text-2xl" />
              </div>
              <div>
                <div className="text-xs text-muted-foreground font-medium">Total Value</div>
                <div className="text-2xl font-bold">{totalSpent.toFixed(2)} BAM</div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      <div className="flex items-center gap-2 p-1 bg-muted rounded-xl w-fit border">
        {["ALL", "CONFIRMED", "PENDING", "CANCELLED"].map((f) => (
          <Button
            key={f}
            variant={filter === f ? "default" : "ghost"}
            size="sm"
            onClick={() => setFilter(f)}
            className="rounded-lg text-xs font-semibold uppercase tracking-wider"
          >
            {f === "ALL" ? "All Bookings" : f}
          </Button>
        ))}
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {filteredBookings?.map((booking) => (
          <Card key={booking.bookingId} className="rounded-2xl border shadow-sm hover:shadow-md transition-shadow overflow-hidden flex flex-col justify-between">
            <div>
              <CardHeader className="pb-3 border-b bg-muted/20">
                <div className="flex items-center justify-between gap-2">
                  <CardTitle className="text-lg font-bold line-clamp-1">{booking.eventName}</CardTitle>
                  {statusBadge(booking.status)}
                </div>
              </CardHeader>
              <CardContent className="space-y-4 pt-4 text-sm">
                <div className="grid grid-cols-2 gap-3 text-xs text-muted-foreground">
                  <div className="flex items-center gap-2">
                    <MdEvent className="text-primary text-base" />
                    <span>{new Date(booking.eventDate).toLocaleDateString("bs-BA")}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <MdLocationOn className="text-primary text-base" />
                    <span>{booking.eventLocation}</span>
                  </div>
                  <div>
                    Category: <strong className="text-foreground font-semibold">{booking.category}</strong>
                  </div>
                  <div>
                    Seats: <strong className="text-foreground font-semibold">{booking.numberOfSeats}</strong>
                  </div>
                </div>

                <div className="flex items-center justify-between border-t pt-3">
                  <span className="text-xs text-muted-foreground font-medium">Total Amount Paid</span>
                  <span className="text-lg font-extrabold text-foreground">{booking.totalPrice.toFixed(2)} BAM</span>
                </div>
              </CardContent>
            </div>

            <div className="px-6 py-3 bg-muted/40 border-t flex items-center justify-end gap-2">
              {booking.status === "Confirmed" && (
                <Button
                  variant="destructive"
                  size="sm"
                  onClick={() => cancelMutation.mutate(booking.bookingId)}
                  disabled={cancelMutation.isPending}
                  className="rounded-lg text-xs"
                >
                  <MdCancel className="mr-1" /> Cancel Booking
                </Button>
              )}

              {booking.qrCodeBase64 && (
                <Dialog>
                  <DialogTrigger>
                    <Button variant="outline" size="sm" className="rounded-lg text-xs font-semibold flex items-center gap-1">
                      <MdQrCode2 className="text-base text-primary" /> View Digital Ticket
                    </Button>
                  </DialogTrigger>
                  <DialogContent className="sm:max-w-md text-center">
                    <DialogHeader>
                      <DialogTitle className="text-xl font-bold text-center">Digital Ticket & QR Code</DialogTitle>
                    </DialogHeader>
                    <div className="space-y-4 py-4 flex flex-col items-center">
                      <div className="bg-white p-4 rounded-2xl shadow-inner border border-gray-200">
                        <img
                          src={`data:image/png;base64,${booking.qrCodeBase64}`}
                          alt="QR Code"
                          className="w-56 h-56"
                        />
                      </div>
                      <div className="text-xs text-muted-foreground space-y-1">
                        <p className="font-semibold text-foreground text-sm">{booking.eventName}</p>
                        <p>{booking.numberOfSeats}x {booking.category} Ticket(s)</p>
                        <p className="text-[11px] font-mono">ID: {booking.bookingId}</p>
                      </div>
                      <Button
                        className="w-full rounded-xl flex items-center justify-center gap-2"
                        onClick={() => downloadQrCode(booking.qrCodeBase64!, booking.eventName)}
                      >
                        <MdDownload className="text-lg" /> Download QR Code PNG
                      </Button>
                    </div>
                  </DialogContent>
                </Dialog>
              )}
            </div>
          </Card>
        ))}
      </div>

      {filteredBookings?.length === 0 && (
        <div className="text-center py-16 bg-muted/20 border rounded-2xl space-y-3">
          <MdBookmark className="mx-auto text-4xl text-muted-foreground/50" />
          <p className="text-lg font-semibold text-foreground">No bookings found</p>
          <p className="text-sm text-muted-foreground">
            You don't have any bookings matching the selected status filter.
          </p>
        </div>
      )}
    </div>
  );
}
