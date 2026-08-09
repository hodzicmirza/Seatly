import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { MdLogin, MdPersonAdd } from "react-icons/md";
import { FaGoogle, FaGithub } from "react-icons/fa";

export default function LoginPage() {
  const { signIn, signUp, signInWithOAuth } = useAuth();
  const navigate = useNavigate();
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isRegister, setIsRegister] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);

    if (isRegister) {
      if (password !== confirmPassword) {
        setError("Passwords do not match.");
        return;
      }
    }

    setLoading(true);

    if (isRegister) {
      const err = await signUp(email, password, fullName);
      if (err) {
        setError(err);
      } else {
        setSuccess("Account created! Check your email if confirmation is required.");
      }
    } else {
      const err = await signIn(email, password);
      if (err) {
        setError(err);
      } else {
        navigate("/");
      }
    }

    setLoading(false);
  };

  const handleOAuth = async (provider: "google" | "github") => {
    setError(null);
    const err = await signInWithOAuth(provider);
    if (err) {
      if (err.includes("provider is not enabled") || err.includes("validation_failed")) {
        setError(`${provider.toUpperCase()} prijava trenutno nije omogućena u Supabase Dashboardu.`);
      } else {
        setError(err);
      }
    }
  };

  return (
    <div className="flex justify-center items-center min-h-[70vh]">
      <Card className="w-full max-w-md shadow-md">
        <CardHeader className="text-center">
          <CardTitle className="text-2xl font-bold">
            {isRegister ? "Create Account" : "Welcome Back"}
          </CardTitle>
          <CardDescription>
            {isRegister
              ? "Fill in your details below to register for Seatly"
              : "Sign in with your email and password"}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <Button
              variant="outline"
              type="button"
              className="w-full flex items-center justify-center gap-2"
              onClick={() => handleOAuth("google")}
            >
              <FaGoogle className="text-red-500" /> Google
            </Button>
            <Button
              variant="outline"
              type="button"
              className="w-full flex items-center justify-center gap-2"
              onClick={() => handleOAuth("github")}
            >
              <FaGithub /> GitHub
            </Button>
          </div>

          <div className="relative my-2">
            <div className="absolute inset-0 flex items-center">
              <Separator />
            </div>
            <div className="relative flex justify-center text-xs uppercase">
              <span className="bg-background px-2 text-muted-foreground">Or continue with</span>
            </div>
          </div>

          <form onSubmit={handleSubmit} className="space-y-4">
            {isRegister && (
              <div>
                <Label htmlFor="fullName">Full Name</Label>
                <Input
                  id="fullName"
                  type="text"
                  placeholder="e.g. Mirza Hodžić"
                  value={fullName}
                  onChange={(e) => setFullName(e.target.value)}
                  required={isRegister}
                  className="mt-1"
                />
              </div>
            )}

            <div>
              <Label htmlFor="email">Email Address</Label>
              <Input
                id="email"
                type="email"
                placeholder="name@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                className="mt-1"
              />
            </div>

            <div>
              <Label htmlFor="password">Password</Label>
              <Input
                id="password"
                type="password"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                minLength={6}
                className="mt-1"
              />
            </div>

            {isRegister && (
              <div>
                <Label htmlFor="confirmPassword">Confirm Password</Label>
                <Input
                  id="confirmPassword"
                  type="password"
                  placeholder="••••••••"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  required={isRegister}
                  minLength={6}
                  className="mt-1"
                />
              </div>
            )}

            {error && <p className="text-sm font-medium text-destructive text-center">{error}</p>}
            {success && <p className="text-sm font-medium text-green-600 text-center">{success}</p>}

            <Button type="submit" className="w-full" disabled={loading}>
              {isRegister ? (
                <><MdPersonAdd className="mr-2" /> Register</>
              ) : (
                <><MdLogin className="mr-2" /> Sign In</>
              )}
            </Button>

            <Button
              type="button"
              variant="ghost"
              className="w-full text-muted-foreground"
              onClick={() => {
                setIsRegister(!isRegister);
                setError(null);
                setSuccess(null);
              }}
            >
              {isRegister ? "Already have an account? Sign In" : "Don't have an account? Register"}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
