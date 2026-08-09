import { createContext, useContext, useEffect, useState } from "react";
import type { User, Session } from "@supabase/supabase-js";
import { supabase } from "@/lib/supabase";
import api from "@/api/client";

export interface DbUser {
  id: string;
  supabaseUserId: string;
  fullName: string;
  email: string;
  role: string;
  createdAt: string;
}

interface AuthContextType {
  user: User | null;
  session: Session | null;
  dbUser: DbUser | null;
  loading: boolean;
  signUp: (email: string, password: string, fullName?: string) => Promise<string | null>;
  signIn: (email: string, password: string) => Promise<string | null>;
  signInWithOAuth: (provider: "google" | "github") => Promise<string | null>;
  signOut: () => Promise<void>;
  refetchDbUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [session, setSession] = useState<Session | null>(null);
  const [dbUser, setDbUser] = useState<DbUser | null>(null);
  const [loading, setLoading] = useState(true);

  const syncUserToBackend = async (extraName?: string) => {
    try {
      const res = await api.get<DbUser>("/users/me");
      if (extraName && (res.data.fullName !== extraName || res.data.fullName.includes("@"))) {
        const syncRes = await api.post<DbUser>("/users/sync", { fullName: extraName });
        setDbUser(syncRes.data);
      } else {
        setDbUser(res.data);
      }
    } catch (e) {
    }
  };

  const refetchDbUser = async () => {
    try {
      const res = await api.get<DbUser>("/users/me");
      setDbUser(res.data);
    } catch (e) {
    }
  };

  useEffect(() => {
    supabase.auth.getSession().then(({ data: { session } }) => {
      setSession(session);
      setUser(session?.user ?? null);
      setLoading(false);
      if (session) {
        syncUserToBackend(session.user?.user_metadata?.full_name);
      }
    });

    const { data: { subscription } } = supabase.auth.onAuthStateChange((_event, session) => {
      setSession(session);
      setUser(session?.user ?? null);
      if (session) {
        syncUserToBackend(session.user?.user_metadata?.full_name);
      } else {
        setDbUser(null);
      }
    });

    return () => subscription.unsubscribe();
  }, []);

  const signUp = async (email: string, password: string, fullName?: string): Promise<string | null> => {
    const { error, data } = await supabase.auth.signUp({
      email,
      password,
      options: {
        data: { full_name: fullName },
      },
    });
    if (data.session) {
      syncUserToBackend(fullName);
    }
    return error?.message ?? null;
  };

  const signIn = async (email: string, password: string): Promise<string | null> => {
    const { error, data } = await supabase.auth.signInWithPassword({ email, password });
    if (data.session) {
      syncUserToBackend();
    }
    return error?.message ?? null;
  };

  const signInWithOAuth = async (provider: "google" | "github"): Promise<string | null> => {
    const { error } = await supabase.auth.signInWithOAuth({
      provider,
      options: {
        redirectTo: window.location.origin,
      },
    });
    return error?.message ?? null;
  };

  const signOut = async () => {
    await supabase.auth.signOut();
    localStorage.removeItem("seatly_token");
    setSession(null);
    setUser(null);
    setDbUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        session,
        dbUser,
        loading,
        signUp,
        signIn,
        signInWithOAuth,
        signOut,
        refetchDbUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within AuthProvider");
  return context;
}
