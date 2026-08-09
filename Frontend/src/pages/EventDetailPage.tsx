import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useParams, Link, useNavigate } from "react-router-dom";
import { useState } from "react";
import { getEventById, deleteEvent } from "@/api/events";
import { createBooking } from "@/api/bookings";
import { useAuth } from "@/context/AuthContext";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
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
  MdLocationOn,
  MdEventSeat,
  MdArrowBack,
  MdConfirmationNumber,
  MdLocalOffer,
  MdCheckCircle,
  MdEdit,
  MdDelete,
} from "react-icons/md";

export default function EventDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { dbUser } = useAuth();

  const [seats, setSeats] = useState(1);
  const [category, setCategory] = useState("");
  const [open, setOpen] = useState(false);

  const isAdminOrOrganizer = dbUser?.role === "Admin" || dbUser?.role === "Organizer";

  const { data: event, isLoading, error } = useQuery({
    queryKey: ["event", id],
    queryFn: () => getEventById(id!),
    enabled: !!id,
  });

  const bookingMutation = useMutation({
    mutationFn: createBooking,
    onSuccess: () => {
      toast.success("Booking created successfully!");
      setOpen(false);
      queryClient.invalidateQueries({ queryKey: ["event", id] });
      queryClient.invalidateQueries({ queryKey: ["bookings"] });
      navigate("/");
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.error || "Booking failed.");
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () => deleteEvent(id!),
    onSuccess: () => {
      toast.success("Event deleted successfully.");
      navigate("/");
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.error || "Failed to delete event.");
    },
  });

  if (isLoading) return <p className="text-muted-foreground">Loading event details...</p>;
  if (error || !event) return <p className="text-destructive">Event not found.</p>;

  const selectedCatObj = event.categories.find((c) => c.name === category);
  const rawUnitPrice = selectedCatObj ? selectedCatObj.finalPrice : 0;
  const isBulkDiscount = seats >= 5;
  const unitPrice = isBulkDiscount ? rawUnitPrice * 0.85 : rawUnitPrice;
  const totalPrice = unitPrice * seats;

  return (
    <div className="space-y-6 max-w-2xl mx-auto">
      <div className="flex items-center justify-between">
        <Button variant="ghost" type="button" onClick={() => navigate("/")}>
          <MdArrowBack className="mr-2" /> Back to Events
        </Button>

        {isAdminOrOrganizer && (
          <div className="flex items-center gap-2">
            <Link to={`/events/${id}/edit`}>
              <Button variant="outline" size="sm" className="rounded-lg">
                <MdEdit className="mr-1.5" /> Edit Event
              </Button>
            </Link>
            <Button
              variant="destructive"
              size="sm"
              className="rounded-lg"
              disabled={deleteMutation.isPending}
              onClick={() => {
                if (confirm("Are you sure you want to delete this event?")) {
                  deleteMutation.mutate();
                }
              }}
            >
              <MdDelete className="mr-1.5" /> {deleteMutation.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        )}
      </div>

      <Card className="shadow-md">
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="text-2xl font-bold">{event.name}</CardTitle>
            <Badge variant="secondary" className="px-3 py-1 text-sm">{event.eventType}</Badge>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-muted-foreground leading-relaxed">{event.description}</p>

          <Separator />

          <div className="grid grid-cols-2 gap-4 text-sm">
            <div className="flex items-center gap-2">
              <MdEvent className="text-primary text-lg" />
              <span>{new Date(event.date).toLocaleString("en-GB")}</span>
            </div>
            <div className="flex items-center gap-2">
              <MdLocationOn className="text-primary text-lg" />
              <span>{event.city}, {event.country}</span>
            </div>
            <div className="flex items-center gap-2">
              <MdEventSeat className="text-primary text-lg" />
              <span>{event.availableSeats} / {event.totalSeats} seats available</span>
            </div>
            <div className="flex items-center gap-2">
              <MdConfirmationNumber className="text-primary text-lg" />
              <span>Starting from <strong>{event.basePrice.toFixed(2)} BAM</strong></span>
            </div>
          </div>

          <Separator />

          <div className="bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 dark:border-emerald-800 rounded-lg p-3.5 flex items-start gap-3">
            <MdLocalOffer className="text-emerald-600 dark:text-emerald-400 text-xl mt-0.5" />
            <div className="text-sm">
              <span className="font-semibold text-emerald-800 dark:text-emerald-300">Special Discount Available!</span>
              <p className="text-emerald-700 dark:text-emerald-400">
                Book <strong>5 or more seats</strong> to automatically unlock a <strong>15% Bulk Discount</strong> on your entire reservation!
              </p>
            </div>
          </div>

          <div>
            <div className="flex items-center justify-between mb-2">
              <h3 className="font-semibold text-base">Seat Categories</h3>
              <span className="text-xs text-muted-foreground">Total Capacity: {event.totalSeats} seats</span>
            </div>
            <div className="grid gap-2.5">
              {event.categories.map((cat) => (
                <div
                  key={cat.name}
                  className="flex items-center justify-between p-3.5 border rounded-xl bg-background hover:border-primary transition-colors"
                >
                  <div className="space-y-0.5">
                    <span className="font-semibold text-foreground block">{cat.name}</span>
                    {cat.seatsCount > 0 && (
                      <span className="text-xs text-muted-foreground font-medium">
                        Capacity: <strong>{cat.seatsCount} seats</strong>
                      </span>
                    )}
                  </div>
                  <span className="text-primary font-bold text-base">
                    {cat.finalPrice.toFixed(2)} BAM
                  </span>
                </div>
              ))}
            </div>
          </div>

          <Dialog open={open} onOpenChange={setOpen}>
            <DialogTrigger>
              <Button className="w-full text-base py-6 font-semibold rounded-xl shadow-sm" disabled={event.availableSeats === 0}>
                {event.availableSeats === 0 ? "Sold Out" : "Book Seats Now"}
              </Button>
            </DialogTrigger>
            <DialogContent className="sm:max-w-md rounded-2xl">
              <DialogHeader>
                <DialogTitle className="text-xl font-bold">Book Tickets: {event.name}</DialogTitle>
              </DialogHeader>
              <div className="space-y-4 pt-2">
                <div>
                  <Label className="text-sm font-semibold mb-2 block">Select Seat Category *</Label>
                  <div className="space-y-2 max-h-60 overflow-y-auto pr-1">
                    {event.categories.map((cat) => {
                      const isSelected = category === cat.name;
                      return (
                        <button
                          key={cat.name}
                          type="button"
                          onClick={() => setCategory(cat.name)}
                          className={`w-full flex items-center justify-between p-3.5 rounded-xl border text-left transition-all ${
                            isSelected
                              ? "border-primary bg-primary/10 ring-2 ring-primary/30 font-semibold"
                              : "border-border hover:border-muted-foreground/40 bg-background"
                          }`}
                        >
                          <div className="space-y-0.5">
                            <div className="flex items-center gap-2">
                              <div
                                className={`w-4 h-4 rounded-full border flex items-center justify-center ${
                                  isSelected ? "border-primary bg-primary text-primary-foreground" : "border-muted-foreground/40"
                                }`}
                              >
                                {isSelected && <div className="w-1.5 h-1.5 rounded-full bg-background" />}
                              </div>
                              <span className="font-semibold text-foreground">{cat.name}</span>
                            </div>
                            {cat.seatsCount > 0 && (
                              <p className="text-xs text-muted-foreground pl-6">
                                Capacity: <strong>{cat.seatsCount} seats</strong>
                              </p>
                            )}
                          </div>
                          <span className="font-bold text-sm text-primary">
                            {cat.finalPrice.toFixed(2)} BAM
                          </span>
                        </button>
                      );
                    })}
                  </div>
                </div>

                <div>
                  <Label className="text-sm font-semibold">Number of Seats *</Label>
                  <Input
                    type="number"
                    min={1}
                    max={event.availableSeats}
                    value={seats}
                    onChange={(e) => setSeats(Number(e.target.value))}
                    className="mt-1"
                  />
                  <p className="text-[11px] text-muted-foreground mt-1">
                    Available for event: {event.availableSeats} seats
                  </p>
                </div>

                {isBulkDiscount && (
                  <div className="flex items-center gap-2 text-sm text-emerald-700 dark:text-emerald-300 font-medium bg-emerald-50 dark:bg-emerald-950/40 p-3 rounded-xl border border-emerald-200 dark:border-emerald-800">
                    <MdCheckCircle className="text-lg text-emerald-600 dark:text-emerald-400" />
                    <span>15% Bulk Discount applied for ordering {seats} seats!</span>
                  </div>
                )}

                {category && (
                  <div className="border-t pt-3 space-y-1 text-sm bg-muted/30 p-3 rounded-xl">
                    <div className="flex justify-between text-muted-foreground">
                      <span>Category Price:</span>
                      <span>{rawUnitPrice.toFixed(2)} BAM</span>
                    </div>
                    {isBulkDiscount && (
                      <div className="flex justify-between text-emerald-600 font-medium">
                        <span>Discount (15%):</span>
                        <span>-{(rawUnitPrice * 0.15).toFixed(2)} BAM / seat</span>
                      </div>
                    )}
                    <div className="flex justify-between font-bold text-base pt-1 text-foreground">
                      <span>Total ({seats} seats):</span>
                      <span className="text-primary">{totalPrice.toFixed(2)} BAM</span>
                    </div>
                  </div>
                )}

                <Button
                  className="w-full text-base py-6 font-semibold rounded-xl shadow-md"
                  disabled={!category || seats < 1 || bookingMutation.isPending}
                  onClick={() =>
                    bookingMutation.mutate({
                      eventId: event.id,
                      numberOfSeats: seats,
                      categoryName: category,
                    })
                  }
                >
                  {bookingMutation.isPending ? "Processing..." : `Confirm Booking (${totalPrice.toFixed(2)} BAM)`}
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        </CardContent>
      </Card>
    </div>
  );
}
