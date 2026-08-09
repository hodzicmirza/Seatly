import { useState, useEffect } from "react";
import { useAuth } from "@/context/AuthContext";
import api from "@/api/client";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { toast } from "sonner";
import { MdPerson, MdEmail, MdBadge, MdCalendarToday, MdSave } from "react-icons/md";

export default function ProfilePage() {
  const { dbUser, user, refetchDbUser } = useAuth();
  const [fullName, setFullName] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (dbUser?.fullName) {
      setFullName(dbUser.fullName);
    }
  }, [dbUser]);

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!fullName.trim()) {
      toast.error("Full name cannot be empty.");
      return;
    }

    setSaving(true);
    try {
      await api.put("/users/me", {
        fullName: fullName.trim(),
        email: dbUser?.email || user?.email || "",
      });
      await refetchDbUser();
      toast.success("Profile updated successfully!");
    } catch (err: any) {
      toast.error(err.response?.data?.error || "Failed to update profile.");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="max-w-xl mx-auto space-y-6">
      <h1 className="text-2xl font-bold">My Profile</h1>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="text-xl">User Details</CardTitle>
              <CardDescription>View and manage your account information</CardDescription>
            </div>
            {dbUser?.role && (
              <Badge variant={dbUser.role === "Admin" ? "default" : "secondary"}>
                {dbUser.role}
              </Badge>
            )}
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <form onSubmit={handleUpdate} className="space-y-4">
            <div>
              <Label htmlFor="fullName" className="flex items-center gap-1.5 mb-1">
                <MdPerson className="text-muted-foreground" /> Full Name
              </Label>
              <Input
                id="fullName"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                placeholder="Enter your full name"
              />
            </div>

            <div>
              <Label className="flex items-center gap-1.5 mb-1 text-muted-foreground">
                <MdEmail /> Email Address
              </Label>
              <Input
                value={dbUser?.email || user?.email || ""}
                disabled
                className="bg-muted text-muted-foreground cursor-not-allowed"
              />
            </div>

            <div className="grid grid-cols-2 gap-4 text-sm pt-2">
              <div className="flex items-center gap-2 text-muted-foreground">
                <MdBadge />
                <span>Role: <strong className="text-foreground">{dbUser?.role || "Customer"}</strong></span>
              </div>
              <div className="flex items-center gap-2 text-muted-foreground">
                <MdCalendarToday />
                <span>Joined: <strong className="text-foreground">{dbUser?.createdAt ? new Date(dbUser.createdAt).toLocaleDateString("bs-BA") : "N/A"}</strong></span>
              </div>
            </div>

            <Button type="submit" className="w-full" disabled={saving}>
              <MdSave className="mr-2" /> {saving ? "Saving..." : "Save Changes"}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
