import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useQuery, useMutation } from "@tanstack/react-query";
import { getEventById, updateEvent } from "@/api/events";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { toast } from "sonner";
import { MdArrowBack, MdSave, MdEvent, MdLocationOn, MdConfirmationNumber, MdAdd, MdDelete, MdHelpOutline } from "react-icons/md";

export default function EditEventPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: event, isLoading, isError } = useQuery({
    queryKey: ["event", id],
    queryFn: () => getEventById(id!),
    enabled: !!id,
  });

  const [form, setForm] = useState<{
    name: string;
    description: string;
    date: string;
    street: string;
    city: string;
    country: string;
    basePrice: number;
    totalSeats: number;
    categories: Array<{ name: string; multiplier: number; seatsCount: number }>;
  }>({
    name: "",
    description: "",
    date: "",
    street: "",
    city: "",
    country: "",
    basePrice: 0,
    totalSeats: 0,
    categories: [{ name: "Standard", multiplier: 1.0, seatsCount: 0 }],
  });

  useEffect(() => {
    if (event) {
      const d = new Date(event.date);
      const isoDate = new Date(d.getTime() - d.getTimezoneOffset() * 60000)
        .toISOString()
        .slice(0, 16);

      const loadedCategories =
        event.categories && event.categories.length > 0
          ? event.categories.map((c) => ({
              name: c.name,
              multiplier: c.priceMultiplier ?? 1.0,
              seatsCount: c.seatsCount || 0,
            }))
          : [{ name: "Standard", multiplier: 1.0, seatsCount: event.totalSeats || 0 }];

      setForm({
        name: event.name || "",
        description: event.description || "",
        date: isoDate,
        street: event.street || "",
        city: event.city || "",
        country: event.country || "",
        basePrice: event.basePrice || 0,
        totalSeats: event.totalSeats || 0,
        categories: loadedCategories,
      });
    }
  }, [event]);

  const allocatedSeats = form.categories.reduce((acc, c) => acc + (Number(c.seatsCount) || 0), 0);
  const unallocatedSeats = form.totalSeats - allocatedSeats;

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

  const addCategory = () => {
    const defaultSeats = unallocatedSeats > 0 ? unallocatedSeats : 0;
    setForm((prev) => ({
      ...prev,
      categories: [...prev.categories, { name: "", multiplier: 1.0, seatsCount: defaultSeats }],
    }));
  };

  const removeCategory = (index: number) => {
    if (form.categories.length <= 1) {
      toast.error("At least one category is required.");
      return;
    }
    setForm((prev) => ({
      ...prev,
      categories: prev.categories.filter((_, i) => i !== index),
    }));
  };

  const updateCategory = (index: number, field: string, value: any) => {
    setForm((prev) => ({
      ...prev,
      categories: prev.categories.map((c, i) =>
        i === index ? { ...c, [field]: value } : c
      ),
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (allocatedSeats !== form.totalSeats) {
      toast.error(`The sum of category seats (${allocatedSeats}) must equal the total event capacity (${form.totalSeats})! Unallocated seats: ${unallocatedSeats}`);
      return;
    }
    mutation.mutate(form);
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
                Update event information, seat distribution per category, date, or venue location.
              </CardDescription>
            </div>
            <Badge variant="outline" className="px-3 py-1 text-xs">
              {event.eventType}
            </Badge>
          </div>
        </CardHeader>
        <CardContent className="pt-6">
          <form className="space-y-6" onSubmit={handleSubmit}>
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
                  <Label htmlFor="date">Date & Time (DD/MM/YYYY — 24h) *</Label>
                  <Input
                    id="date"
                    type="datetime-local"
                    lang="en-GB"
                    required
                    value={form.date}
                    onChange={(e) => updateField("date", e.target.value)}
                    className="mt-1"
                  />
                  <p className="text-[11px] text-muted-foreground mt-1 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-1">
                    <span>European 24-hour format (DD/MM/YYYY HH:mm)</span>
                    {form.date && !isNaN(new Date(form.date).getTime()) && (
                      <span className="font-semibold text-primary">
                        Preview: {new Date(form.date).toLocaleString("en-GB", { day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit", hour12: false })}
                      </span>
                    )}
                  </p>
                </div>
                <div>
                  <Label htmlFor="totalSeats">Total Capacity (Max Seats) *</Label>
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

            <div className="space-y-4 pt-4 border-t">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="font-semibold text-sm text-primary uppercase tracking-wider flex items-center gap-1.5">
                    <MdConfirmationNumber className="text-lg" /> Seat Categories & Seat Distribution
                  </h3>
                  <p className="text-xs text-muted-foreground mt-0.5">
                    Specify the number of seats allocated to each category. Total category seats must equal Total Capacity ({form.totalSeats}).
                  </p>
                </div>
                <Button type="button" variant="outline" size="sm" onClick={addCategory} className="rounded-lg">
                  <MdAdd className="mr-1" /> Add Category
                </Button>
              </div>

              {/* Unallocated Seats Status Banner */}
              <div
                className={`p-3.5 rounded-xl border text-xs sm:text-sm font-semibold flex flex-col sm:flex-row items-start sm:items-center justify-between gap-2 transition-colors ${
                  unallocatedSeats === 0
                    ? "bg-emerald-50 text-emerald-800 border-emerald-300 dark:bg-emerald-950/40 dark:text-emerald-300 dark:border-emerald-800"
                    : unallocatedSeats > 0
                    ? "bg-amber-50 text-amber-800 border-amber-300 dark:bg-amber-950/40 dark:text-amber-300 dark:border-amber-800"
                    : "bg-red-50 text-red-800 border-red-300 dark:bg-red-950/40 dark:text-red-300 dark:border-red-800"
                }`}
              >
                <div>
                  Total Allocated: <strong>{allocatedSeats}</strong> / {form.totalSeats} seats
                </div>
                <div>
                  {unallocatedSeats === 0 ? (
                    <span className="text-emerald-700 dark:text-emerald-400 font-bold flex items-center gap-1">
                      ✓ All seats successfully allocated!
                    </span>
                  ) : unallocatedSeats > 0 ? (
                    <span className="text-amber-800 dark:text-amber-300 font-bold">
                      ⚠️ Unallocated seats: <strong>{unallocatedSeats}</strong>
                    </span>
                  ) : (
                    <span className="text-red-700 dark:text-red-400 font-bold">
                      ❌ Capacity exceeded by {Math.abs(unallocatedSeats)} seats!
                    </span>
                  )}
                </div>
              </div>

              <div className="bg-muted/40 border p-3 rounded-xl text-xs space-y-1">
                <div className="flex items-center gap-1 font-semibold text-foreground">
                  <MdHelpOutline className="text-primary text-sm" /> Multipliers & Base Price
                </div>
                <p className="text-muted-foreground leading-relaxed">
                  Base Price: <strong>{form.basePrice || 0} BAM</strong> | Total Capacity: <strong>{form.totalSeats} seats</strong>
                </p>
              </div>

              <div className="space-y-3">
                <div className="grid grid-cols-12 gap-2 sm:gap-3 text-xs font-semibold text-muted-foreground px-1">
                  <div className="col-span-4">Category Name</div>
                  <div className="col-span-3">Capacity (Seats)</div>
                  <div className="col-span-4">Price Multiplier</div>
                  <div className="col-span-1 text-right">Action</div>
                </div>

                {form.categories.map((cat, i) => {
                  const finalPrice = (form.basePrice || 0) * (cat.multiplier || 1);
                  return (
                    <div key={i} className="grid grid-cols-12 gap-2 sm:gap-3 items-center bg-background border p-2.5 rounded-xl">
                      <div className="col-span-4">
                        <Input
                          placeholder="e.g. Standard, VIP"
                          value={cat.name}
                          onChange={(e) => updateCategory(i, "name", e.target.value)}
                        />
                      </div>
                      <div className="col-span-3">
                        <Input
                          type="number"
                          min={1}
                          placeholder="Seats"
                          value={cat.seatsCount || ""}
                          onChange={(e) => updateCategory(i, "seatsCount", Number(e.target.value))}
                        />
                      </div>
                      <div className="col-span-4 flex items-center gap-1.5">
                        <Input
                          type="number"
                          step="0.1"
                          min="0.1"
                          placeholder="1.0"
                          value={cat.multiplier}
                          onChange={(e) => updateCategory(i, "multiplier", Number(e.target.value))}
                          className="w-16 sm:w-20"
                        />
                        <span className="text-[11px] sm:text-xs font-semibold text-foreground bg-muted px-1.5 sm:px-2 py-1 rounded truncate">
                          = {finalPrice.toFixed(2)} BAM
                        </span>
                      </div>
                      <div className="col-span-1 text-right">
                        {form.categories.length > 1 && (
                          <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            onClick={() => removeCategory(i)}
                            className="text-destructive hover:bg-destructive/10 px-2"
                          >
                            <MdDelete className="text-base" />
                          </Button>
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>

            <Button
              type="submit"
              className="w-full text-base py-6 font-semibold rounded-xl shadow-md mt-6"
              disabled={mutation.isPending || unallocatedSeats !== 0}
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
