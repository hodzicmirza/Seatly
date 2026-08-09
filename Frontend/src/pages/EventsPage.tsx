import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Link } from "react-router-dom";
import { getAllEvents, searchEvents } from "@/api/events";
import { useAuth } from "@/context/AuthContext";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  MdEvent,
  MdLocationOn,
  MdSearch,
  MdAdd,
  MdEventSeat,
  MdMusicNote,
  MdBusinessCenter,
  MdConfirmationNumber,
  MdStar,
} from "react-icons/md";

export default function EventsPage() {
  const { dbUser } = useAuth();
  const [search, setSearch] = useState("");
  const [selectedType, setSelectedType] = useState<string>("ALL");

  const canCreateEvent = dbUser?.role === "Admin" || dbUser?.role === "Organizer";

  const { data: events, isLoading, error } = useQuery({
    queryKey: ["events", search, selectedType],
    queryFn: async () => {
      const all = search ? await searchEvents({ name: search }) : await getAllEvents();
      if (selectedType === "ALL") return all;
      return all.filter((e) => e.eventType.toUpperCase() === selectedType.toUpperCase());
    },
  });

  const totalEvents = events?.length || 0;
  const totalSeatsAvailable = events?.reduce((sum, e) => sum + e.availableSeats, 0) || 0;

  return (
    <div className="space-y-8 pb-8">
      <div className="relative overflow-hidden rounded-2xl bg-gradient-to-r from-violet-600 via-indigo-600 to-purple-700 text-white p-8 md:p-12 shadow-xl">
        <div className="relative z-10 space-y-4 max-w-2xl">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-white/10 backdrop-blur-md border border-white/20 text-xs font-semibold uppercase tracking-wider text-purple-200">
            <MdStar className="text-yellow-400" /> Premium Event Booking Platform
          </div>
          <h1 className="text-3xl md:text-5xl font-extrabold tracking-tight leading-tight">
            Discover & Book Unforgettable Experiences
          </h1>
          <p className="text-purple-100 text-sm md:text-base opacity-90 leading-relaxed">
            Reserve your seats instantly for top concerts, tech conferences, and exclusive live events with real-time seat tracking and instant QR tickets.
          </p>
          <div className="flex flex-wrap items-center gap-6 pt-2 text-sm font-medium">
            <div className="flex items-center gap-2">
              <span className="text-2xl font-bold text-white">{totalEvents}</span> Active Events
            </div>
            <div className="h-4 w-px bg-white/30" />
            <div className="flex items-center gap-2">
              <span className="text-2xl font-bold text-white">{totalSeatsAvailable}</span> Seats Available
            </div>
          </div>
        </div>

        <div className="absolute -right-12 -bottom-12 w-80 h-80 rounded-full bg-white/10 blur-3xl pointer-events-none" />
        <div className="absolute right-40 -top-12 w-60 h-60 rounded-full bg-purple-400/20 blur-2xl pointer-events-none" />
      </div>

      <div className="flex flex-col md:flex-row items-stretch md:items-center justify-between gap-4">
        <div className="relative flex-1 max-w-md">
          <MdSearch className="absolute left-3.5 top-3.5 text-muted-foreground text-lg" />
          <Input
            placeholder="Search events by title or keyword..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-10 h-11 rounded-xl border-muted shadow-sm focus-visible:ring-primary"
          />
        </div>

        <div className="flex items-center justify-between gap-3">
          <div className="flex items-center gap-1.5 p-1 bg-muted rounded-xl border">
            <Button
              variant={selectedType === "ALL" ? "default" : "ghost"}
              size="sm"
              onClick={() => setSelectedType("ALL")}
              className="rounded-lg text-xs font-semibold"
            >
              All Events
            </Button>
            <Button
              variant={selectedType === "CONCERT" ? "default" : "ghost"}
              size="sm"
              onClick={() => setSelectedType("CONCERT")}
              className="rounded-lg text-xs font-semibold flex items-center gap-1"
            >
              <MdMusicNote /> Concerts
            </Button>
            <Button
              variant={selectedType === "CONFERENCE" ? "default" : "ghost"}
              size="sm"
              onClick={() => setSelectedType("CONFERENCE")}
              className="rounded-lg text-xs font-semibold flex items-center gap-1"
            >
              <MdBusinessCenter /> Conferences
            </Button>
          </div>

          {canCreateEvent && (
            <Link to="/events/create">
              <Button className="h-11 rounded-xl shadow-md font-semibold px-4 flex items-center gap-2">
                <MdAdd className="text-lg" /> Create Event
              </Button>
            </Link>
          )}
        </div>
      </div>

      {isLoading && (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-64 rounded-2xl bg-muted/60 animate-pulse border" />
          ))}
        </div>
      )}

      {error && (
        <div className="text-center py-12 bg-destructive/10 border border-destructive/20 rounded-2xl text-destructive font-medium">
          Failed to load events. Please ensure your backend service is running.
        </div>
      )}

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        {events?.map((event) => {
          const isConcert = event.eventType.toUpperCase() === "CONCERT";
          return (
            <Link key={event.id} to={`/events/${event.id}`} className="group">
              <Card className="h-full rounded-2xl border border-border/60 shadow-sm hover:shadow-xl hover:border-primary/50 transition-all duration-300 overflow-hidden flex flex-col justify-between group-hover:-translate-y-1">
                <div>
                  <CardHeader className="pb-3">
                    <div className="flex items-center justify-between gap-2 mb-2">
                      <Badge
                        variant="secondary"
                        className={`text-xs px-2.5 py-0.5 rounded-full font-medium flex items-center gap-1 ${
                          isConcert
                            ? "bg-purple-100 text-purple-700 dark:bg-purple-950/60 dark:text-purple-300"
                            : "bg-blue-100 text-blue-700 dark:bg-blue-950/60 dark:text-blue-300"
                        }`}
                      >
                        {isConcert ? <MdMusicNote /> : <MdBusinessCenter />}
                        {event.eventType}
                      </Badge>
                      {event.availableSeats < 10 && event.availableSeats > 0 && (
                        <Badge variant="destructive" className="text-[10px] uppercase font-bold animate-pulse">
                          Almost Sold Out
                        </Badge>
                      )}
                    </div>
                    <CardTitle className="text-xl font-bold group-hover:text-primary transition-colors line-clamp-1">
                      {event.name}
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-3 text-sm text-muted-foreground">
                    <p className="line-clamp-2 text-xs leading-relaxed text-muted-foreground/80">
                      {event.description}
                    </p>

                    <div className="space-y-2 pt-2 border-t border-border/40 text-xs">
                      <div className="flex items-center gap-2">
                        <MdEvent className="text-primary text-base" />
                        <span className="font-medium text-foreground">
                          {new Date(event.date).toLocaleDateString("en-GB", {
                            weekday: "short",
                            year: "numeric",
                            month: "short",
                            day: "numeric",
                          })}
                        </span>
                      </div>
                      <div className="flex items-center gap-2">
                        <MdLocationOn className="text-primary text-base" />
                        <span>{event.city}, {event.country}</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <MdEventSeat className="text-primary text-base" />
                        <span>
                          <strong className="text-foreground font-semibold">{event.availableSeats}</strong> / {event.totalSeats} seats available
                        </span>
                      </div>
                    </div>
                  </CardContent>
                </div>

                <div className="px-6 py-4 bg-muted/30 border-t flex items-center justify-between">
                  <div className="flex items-center gap-1.5 text-xs text-muted-foreground font-medium">
                    <MdConfirmationNumber className="text-primary" /> Starting from
                  </div>
                  <div className="text-lg font-extrabold text-foreground tracking-tight">
                    {event.basePrice.toFixed(2)} <span className="text-xs font-bold text-muted-foreground">BAM</span>
                  </div>
                </div>
              </Card>
            </Link>
          );
        })}
      </div>

      {events?.length === 0 && (
        <div className="text-center py-16 bg-muted/20 border rounded-2xl space-y-3">
          <MdEvent className="mx-auto text-4xl text-muted-foreground/50" />
          <p className="text-lg font-semibold text-foreground">No events found</p>
          <p className="text-sm text-muted-foreground max-w-sm mx-auto">
            Try adjusting your search criteria or category filter to discover available events.
          </p>
        </div>
      )}
    </div>
  );
}
