import { useState } from 'react'
import { Outlet } from "react-router-dom";
import Sidebar from "./Sidebar";
import Header from "./Header";

export default function Layout() {
  const [menuMobile, setMenuMobile] = useState(false)

  return (
    <div className="flex h-screen w-full overflow-hidden">
      {menuMobile && (
        <div
          className="fixed inset-0 z-20 bg-black/50 md:hidden"
          onClick={() => setMenuMobile(false)}
        />
      )}
      <Sidebar aberto={menuMobile} onFechar={() => setMenuMobile(false)} />
      <div className="flex flex-col flex-1 overflow-hidden min-w-0">
        <Header onMenuClick={() => setMenuMobile(v => !v)} />
        <main className="flex-1 overflow-auto px-4 py-6 md:px-14 md:py-12 bg-slate-50">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
