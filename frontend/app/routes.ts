import { type RouteConfig, index, layout, route } from "@react-router/dev/routes";

export default [
    layout("layouts/sidebar-layout.tsx", [
        index("routes/home.tsx"),
        route("projects/:projectId", "routes/project.tsx"),
        route("chats/:chatId", "routes/chat.tsx"),
    ]),
] satisfies RouteConfig;
