import { BrowserRouter, Routes, Route, Link, Navigate, useNavigate } from "react-router-dom";
import { QueryClient, QueryClientProvider, useQueryClient } from "@tanstack/react-query";
import { Toaster } from "@/components/ui/sonner";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { MdEvent, MdBookmark, MdPerson, MdLogout, MdLogin } from "react-icons/md";
import { AuthProvider, useAuth } from "@/context/AuthContext";
import EventsPage from "@/pages/EventsPage";
import EventDetailPage from "@/pages/EventDetailPage";
import CreateEventPage from "@/pages/CreateEventPage";
import BookingsPage from "@/pages/BookingsPage";
import ProfilePage from "@/pages/ProfilePage";
import LoginPage from "@/pages/LoginPage";

import EditEventPage from "@/pages/EditEventPage";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { user, loading } = useAuth();
  if (loading) return <p className="text-muted-foreground">Loading...</p>;
  if (!user) return <Navigate to="/login" />;
  return <>{children}</>;
}

function Navbar() {
  const { user, dbUser, signOut } = useAuth();
  const client = useQueryClient();
  const navigate = useNavigate();

  const handleLogout = async () => {
    client.clear();
    await signOut();
    navigate("/");
  };

  return (
    <header className="border-b bg-background">
      <div className="container mx-auto px-4 py-3 flex items-center justify-between">
        <div className="flex items-center gap-6">
          <Link to="/" className="text-xl font-bold tracking-tight">Seatly</Link>
          <nav className="flex gap-4">
            <Link to="/" className="flex items-center gap-1 text-sm font-medium hover:text-primary">
              <MdEvent /> Events
            </Link>
            {user && (
              <>
                <Link to="/bookings" className="flex items-center gap-1 text-sm font-medium hover:text-primary">
                  <MdBookmark /> My Bookings
                </Link>
                <Link to="/profile" className="flex items-center gap-1 text-sm font-medium hover:text-primary">
                  <MdPerson /> My Profile
                </Link>
              </>
            )}
          </nav>
        </div>
        <div className="flex items-center gap-3">
          {user ? (
            <>
              <Link to="/profile" className="flex items-center gap-2 text-sm hover:underline">
                <span className="font-medium text-foreground">
                  {dbUser?.fullName || user.email}
                </span>
                {dbUser?.role && dbUser.role !== "Customer" && (
                  <Badge variant="secondary" className="text-xs">
                    {dbUser.role}
                  </Badge>
                )}
              </Link>
              <Button variant="ghost" size="sm" onClick={handleLogout}>
                <MdLogout className="mr-1" /> Logout
              </Button>
            </>
          ) : (
            <Link to="/login">
              <Button variant="ghost" size="sm">
                <MdLogin className="mr-1" /> Sign In
              </Button>
            </Link>
          )}
        </div>
      </div>
    </header>
  );
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <BrowserRouter>
          <Navbar />
          <main className="container mx-auto px-4 py-6">
            <Routes>
              <Route path="/" element={<EventsPage />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/events/create" element={<CreateEventPage />} />
              <Route path="/events/:id" element={<EventDetailPage />} />
              <Route path="/events/:id/edit" element={
                <ProtectedRoute><EditEventPage /></ProtectedRoute>
              } />
              <Route path="/bookings" element={
                <ProtectedRoute><BookingsPage /></ProtectedRoute>
              } />
              <Route path="/profile" element={
                <ProtectedRoute><ProfilePage /></ProtectedRoute>
              } />
            </Routes>
          </main>
          <Toaster />
        </BrowserRouter>
      </AuthProvider>
    </QueryClientProvider>
  );
}
