# Messaging Module (Chat & Contact)

## Mục đích
Giao tiếp realtime — live chat (SignalR), floating contact buttons.

## Trách nhiệm
- Live chat khách hàng ↔ admin (SignalR)
- Chat sessions management
- Floating contact buttons (Zalo, Hotline)
- Unread message count (Dashboard)

## Entities

### ChatSession
| Field | Type | Mô tả |
|-------|------|--------|
| Id | Guid | UUID PK |
| GuestId | string | Client ID (anonymous) |
| UnreadCount | int | Tin nhắn chưa đọc |
| LastMessage | string? | Nội dung tin cuối |
| LastSeenAt | DateTime? | Admin xem lần cuối |

### ChatMessage
| Field | Type | Mô tả |
|-------|------|--------|
| Id | Guid | UUID PK |
| SessionId | Guid | FK → ChatSession |
| SenderId | string? | User ID hoặc guest ID |
| IsFromAdmin | bool | Admin gửi hay khách |
| Text | string | Nội dung |
| IsRead | bool | Đã đọc |

## SignalR Hub (planned)
- `/hubs/chat` — WebSocket endpoint
- Methods: `SendMessage`, `JoinSession`, `MarkAsRead`

## Status
**Planned** — Chưa implement. Entities đã có trong DB schema.

## Changelog
| Ngày | Thay đổi |
|------|----------|
| 2026-04-24 | Khởi tạo module documentation |
