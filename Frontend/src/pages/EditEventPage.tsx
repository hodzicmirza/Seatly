import { useState, useEffect } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { useQuery, useMutation } from "@tanstack/react-query";
import { getEventById, updateEvent } from "@/api/events";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { toast } from "sonner";
import { MdArrowBack, MdSave, MdEvent, MdLocationOn } from "react-icons/md";

export default function EditEventPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: event, isLoading, isError } = useQuery({
    queryKey: ["event", id],
    queryFn: () => getEventById(id!),
    enabled: !!id,
  });

  const [form, setForm] = useState({
    name: "",
    description: "",
    date: "",
    street: "",
    city: "",
    country: "",
    basePrice: 0,
    totalSeats: 0,
  });

  useEffect(() => {
    if (event) {
      const d = new Date(event.date);
      const isoDate = new Date(d.getTime() - d.getTimezoneOffset() * 60000)
        .toISOString()
        .slice(0, 16);

      setForm({
        name: event.name || "",
        description: event.description || "",
        date: isoDate,
        street: "",
        city: event.city || "",
        country: event.country || "",
        basePrice: event.basePrice || 0,
        totalSeats: event.totalSeats || 0,
      });
    }
  }, [event]);

  const mutation = useMutation({
    mutationFn: (data: typeof form) => updateEvent(id!, data),
    onSuccess: () => {
      toast.success("Event updated successfully!");
      navigate(`/events/${id}`);
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.error || "Failed to update event.");
    },
  });

  const updateField = (field: string, value: any) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  if (isLoading) {
    return <div className="p-8 text-center text-muted-foreground">Loading event details...</div>;
  }

  if (isError || !event) {
    return <div className="p-8 text-center text-destructive">Event not found or failed to load.</div>;
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6 pb-12">
      <Button variant="ghost" size="sm" type="button" onClick={() => navigate(-1)}>
        <MdArrowBack className="mr-2" /> Back to Event Details
      </Button>

      <Card className="shadow-lg border rounded-2xl overflow-hidden">
        <CardHeader className="bg-muted/30 border-b pb-6">
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="text-2xl font-bold">Edit Event: {event.name}</CardTitle>
              <CardDescription className="mt-1">
                Update event information, capacity, date, or venue location.
              </CardDescription>
            </div>
            <Badge variant="outline" className="px-3 py-1 text-xs">
              {event.eventType}
            </Badge>
          </div>
        </CardHeader>
        <CardContent className="pt-6">
          <form
            className="space-y-6"
            onSubmit={(e) => {
              e.preventDefault();
              mutation.mutate(form);
            }}
          >
            <div className="space-y-4">
              <h3 className="font-semibold text-sm text-primary uppercase tracking-wider flex items-center gap-1.5">
                <MdEvent className="text-lg" /> General Information
              </h3>

              <div>
                <Label htmlFor="name">Event Name *</Label>
                <Input
                  id="name"
                  required
                  value={form.name}
                  onChange={(e) => updateField("name", e.target.value)}
                  className="mt-1"
                />
              </div>

              <div>
                <Label htmlFor="description">Description *</Label>
                <Input
                  id="description"
                  required
                  value={form.description}
                  onChange={(e) => updateField("description", e.target.value)}
                  className="mt-1"
                />
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="date">Date & Time *</Label>
                  <Input
                    id="date"
                    type="datetime-local"
                    required
                    value={form.date}
                    onChange={(e) => updateField("date", e.target.value)}
                    className="mt-1"
                  />
                </div>
                <div>
                  <Label htmlFor="totalSeats">Total Seats *</Label>
                  <Input
                    id="totalSeats"
                    type="number"
                    min={1}
                    required
                    value={form.totalSeats}
                    onChange={(e) => updateField("totalSeats", Number(e.target.value))}
                    className="mt-1"
                  />
                </div>
              </div>
            </div>

            <div className="space-y-4 pt-4 border-t">
              <h3 className="font-semibold text-sm text-primary uppercase tracking-wider flex items-center gap-1.5">
                <MdLocationOn className="text-lg" /> Location & Base Pricing
              </h3>

              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <div>
                  <Label htmlFor="street">Street Address</Label>
                  <Input
                    id="street"
                    value={form.street}
                    onChange={(e) => updateField("street", e.target.value)}
                    className="mt-1"
                  />
                </div>
                <div>
                  <Label htmlFor="city">City *</Label>
                  <Input
                    id="city"
                    required
                    value={form.city}
                    onChange={(e) => updateField("city", e.target.value)}
                    className="mt-1"
                  />
                </div>
                <div>
                  <Label htmlFor="country">Country *</Label>
                  <Input
                    id="country"
                    required
                    value={form.country}
                    onChange={(e) => updateField("country", e.target.value)}
                    className="mt-1"
                  />
                </div>
              </div>

              <div>
                <Label htmlFor="basePrice">Base Price (BAM) *</Label>
                <Input
                  id="basePrice"
                  type="number"
                  step="0.01"
                  min={1}
                  required
                  value={form.basePrice}
                  onChange={(e) => updateField("basePrice", Number(e.target.value))}
                  className="mt-1 max-w-xs"
                />
              </div>
            </div>

            <Button
              type="submit"
              className="w-full text-base py-6 font-semibold rounded-xl shadow-md mt-6"
              disabled={mutation.isPending}
            >
              <MdSave className="mr-2" />
              {mutation.isPending ? "Saving Changes..." : "Save Changes"}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
