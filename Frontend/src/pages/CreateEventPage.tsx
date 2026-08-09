import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useMutation } from "@tanstack/react-query";
import { createEvent } from "@/api/events";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { toast } from "sonner";
import { MdAdd, MdDelete, MdArrowBack, MdHelpOutline, MdEvent, MdLocationOn, MdConfirmationNumber } from "react-icons/md";
import type { CreateEventRequest } from "@/types";

export default function CreateEventPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState<CreateEventRequest>({
    name: "",
    description: "",
    date: "",
    street: "",
    city: "",
    country: "",
    basePrice: 50,
    totalSeats: 100,
    categories: [{ name: "Standard", multiplier: 1.0 }],
    eventType: 1,
    headliner: "",
    supportAct: "",
    organizer: "",
    keynoteSpeaker: "",
  });

  const mutation = useMutation({
    mutationFn: createEvent,
    onSuccess: (data) => {
      toast.success("Event created successfully!");
      navigate(`/events/${data.id}`);
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.error || "Failed to create event.");
    },
  });

  const updateField = (field: string, value: any) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const addCategory = () => {
    setForm((prev) => ({
      ...prev,
      categories: [...prev.categories, { name: "", multiplier: 1.0 }],
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

  return (
    <div className="max-w-3xl mx-auto space-y-6 pb-12">
      <Button variant="ghost" size="sm" type="button" onClick={() => navigate("/")}>
        <MdArrowBack className="mr-2" /> Back to Events
      </Button>

      <Card className="shadow-lg border rounded-2xl overflow-hidden">
        <CardHeader className="bg-muted/30 border-b pb-6">
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="text-2xl font-bold">Create New Event</CardTitle>
              <CardDescription className="mt-1">
                Fill in the event details, ticket categories, and location to publish a new event.
              </CardDescription>
            </div>
            <Badge variant="secondary" className="px-3 py-1 text-xs">
              Admin Portal
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

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="eventType">Event Type</Label>
                  <select
                    id="eventType"
                    className="w-full border rounded-lg p-2.5 mt-1 bg-background text-sm font-medium focus:ring-2 focus:ring-primary"
                    value={form.eventType}
                    onChange={(e) => updateField("eventType", Number(e.target.value))}
                  >
                    <option value={1}>Concert</option>
                    <option value={2}>Conference</option>
                  </select>
                </div>
                <div>
                  <Label htmlFor="name">Event Name *</Label>
                  <Input
                    id="name"
                    required
                    placeholder="e.g. Summer Music Festival 2026"
                    value={form.name}
                    onChange={(e) => updateField("name", e.target.value)}
                    className="mt-1"
                  />
                </div>
              </div>

              <div>
                <Label htmlFor="description">Description *</Label>
                <Input
                  id="description"
                  required
                  placeholder="Brief summary of the event schedule, performers, or goals..."
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
                  <Label htmlFor="totalSeats">Total Capacity (Seats) *</Label>
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
                <MdLocationOn className="text-lg" /> Venue & Base Pricing
              </h3>

              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <div>
                  <Label htmlFor="street">Street Address</Label>
                  <Input
                    id="street"
                    placeholder="Main Street 12"
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
                    placeholder="Sarajevo"
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
                    placeholder="Bosnia and Herzegovina"
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
                <p className="text-xs text-muted-foreground mt-1">
                  Base ticket price used to calculate category prices (Base Price × Category Multiplier).
                </p>
              </div>
            </div>

            {form.eventType === 1 && (
              <div className="space-y-4 pt-4 border-t">
                <h3 className="font-semibold text-sm text-purple-600 dark:text-purple-400 uppercase tracking-wider">
                  Concert Details
                </h3>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="headliner">Headliner Artist / Band *</Label>
                    <Input
                      id="headliner"
                      placeholder="e.g. ColdPlay"
                      value={form.headliner}
                      onChange={(e) => updateField("headliner", e.target.value)}
                      className="mt-1"
                    />
                  </div>
                  <div>
                    <Label htmlFor="supportAct">Support Act (Optional)</Label>
                    <Input
                      id="supportAct"
                      placeholder="e.g. Opening Band"
                      value={form.supportAct}
                      onChange={(e) => updateField("supportAct", e.target.value)}
                      className="mt-1"
                    />
                  </div>
                </div>
              </div>
            )}

            {form.eventType === 2 && (
              <div className="space-y-4 pt-4 border-t">
                <h3 className="font-semibold text-sm text-blue-600 dark:text-blue-400 uppercase tracking-wider">
                  Conference Details
                </h3>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="organizer">Organizer Company / Host *</Label>
                    <Input
                      id="organizer"
                      placeholder="e.g. TechCorp Bosnia"
                      value={form.organizer}
                      onChange={(e) => updateField("organizer", e.target.value)}
                      className="mt-1"
                    />
                  </div>
                  <div>
                    <Label htmlFor="keynoteSpeaker">Keynote Speaker (Optional)</Label>
                    <Input
                      id="keynoteSpeaker"
                      placeholder="e.g. Dr. Jane Doe"
                      value={form.keynoteSpeaker}
                      onChange={(e) => updateField("keynoteSpeaker", e.target.value)}
                      className="mt-1"
                    />
                  </div>
                </div>
              </div>
            )}

            <div className="space-y-4 pt-4 border-t">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="font-semibold text-sm text-primary uppercase tracking-wider flex items-center gap-1.5">
                    <MdConfirmationNumber className="text-lg" /> Seat Categories & Price Multipliers
                  </h3>
                  <p className="text-xs text-muted-foreground mt-0.5">
                    Define seating zones (e.g. Standard, VIP) and their price multiplier relative to Base Price ({form.basePrice || 0} BAM).
                  </p>
                </div>
                <Button type="button" variant="outline" size="sm" onClick={addCategory} className="rounded-lg">
                  <MdAdd className="mr-1" /> Add Category
                </Button>
              </div>

              <div className="bg-muted/40 border p-3 rounded-xl text-xs space-y-1">
                <div className="flex items-center gap-1 font-semibold text-foreground">
                  <MdHelpOutline className="text-primary text-sm" /> How do Seat Multipliers work?
                </div>
                <p className="text-muted-foreground leading-relaxed">
                  <strong>1.0x</strong> = Standard Base Price ({(form.basePrice * 1.0).toFixed(2)} BAM) |{" "}
                  <strong>1.5x</strong> = VIP Zone (+50% = {(form.basePrice * 1.5).toFixed(2)} BAM) |{" "}
                  <strong>2.0x</strong> = Front Row (+100% = {(form.basePrice * 2.0).toFixed(2)} BAM)
                </p>
              </div>

              <div className="space-y-3">
                <div className="grid grid-cols-12 gap-3 text-xs font-semibold text-muted-foreground px-1">
                  <div className="col-span-6">Category Name</div>
                  <div className="col-span-4">Price Multiplier (Multiplier × Base)</div>
                  <div className="col-span-2 text-right">Action</div>
                </div>

                {form.categories.map((cat, i) => {
                  const finalPrice = (form.basePrice || 0) * (cat.multiplier || 1);
                  return (
                    <div key={i} className="grid grid-cols-12 gap-3 items-center bg-background border p-2.5 rounded-xl">
                      <div className="col-span-6">
                        <Input
                          placeholder="e.g. Standard, VIP, Fan Pit"
                          value={cat.name}
                          onChange={(e) => updateCategory(i, "name", e.target.value)}
                        />
                      </div>
                      <div className="col-span-4 flex items-center gap-2">
                        <Input
                          type="number"
                          step="0.1"
                          min="0.1"
                          placeholder="1.0"
                          value={cat.multiplier}
                          onChange={(e) => updateCategory(i, "multiplier", Number(e.target.value))}
                          className="w-20"
                        />
                        <span className="text-xs font-semibold text-foreground bg-muted px-2 py-1 rounded">
                          = {finalPrice.toFixed(2)} BAM
                        </span>
                      </div>
                      <div className="col-span-2 text-right">
                        {form.categories.length > 1 && (
                          <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            onClick={() => removeCategory(i)}
                            className="text-destructive hover:bg-destructive/10"
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
              disabled={mutation.isPending}
            >
              {mutation.isPending ? "Publishing Event..." : "Publish Event"}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
