import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  getUnreadCount,
  listInboxMessages,
  listSentMessages,
  markMessageRead,
  sendChefMessage,
} from "@/lib/messages";

const baseUrl = "http://localhost:5050";

const sampleMessage = {
  id: "8f2c9a30-5b7e-4d1c-9f3a-6a1e2b4c7d90",
  chefProfileId: "17994471-e812-4da7-ae46-441555e5f09a",
  chefDisplayName: "Amna's Kitchen",
  senderUserId: "c11a1f2e-93b0-4a44-8f7c-2f6f0d5b8e21",
  senderName: "Bilal Khan",
  body: "Is the biryani available on Friday?",
  readAtUtc: null,
  createdAtUtc: "2026-08-22T10:00:00Z",
};

function jsonResponse(status: number, body?: unknown): Response {
  return new Response(body === undefined ? null : JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("messages lib", () => {
  const fetchMock = vi.fn();

  beforeEach(() => {
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    fetchMock.mockReset();
  });

  it("sendChefMessage posts the message and returns it", async () => {
    fetchMock.mockResolvedValue(jsonResponse(201, { data: sampleMessage, meta: null }));

    const result = await sendChefMessage({
      chefProfileId: sampleMessage.chefProfileId,
      body: sampleMessage.body,
    });

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/messages`);
    expect(init.method).toBe("POST");
    expect(JSON.parse(init.body)).toEqual({
      chefProfileId: sampleMessage.chefProfileId,
      body: sampleMessage.body,
    });
    expect(result).toEqual(sampleMessage);
  });

  it("listInboxMessages fetches the inbox with pagination", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(200, {
        data: [sampleMessage],
        meta: { page: 1, pageSize: 20, total: 1, hasMore: false },
      })
    );

    const result = await listInboxMessages(1, 20);

    const calledUrl = fetchMock.mock.calls[0][0] as string;
    expect(calledUrl).toBe(`${baseUrl}/api/messages/inbox?page=1&pageSize=20`);
    expect(result.items).toEqual([sampleMessage]);
    expect(result.total).toBe(1);
  });

  it("listSentMessages fetches sent messages with pagination", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(200, {
        data: [sampleMessage],
        meta: { page: 2, pageSize: 10, total: 11, hasMore: true },
      })
    );

    const result = await listSentMessages(2, 10);

    const calledUrl = fetchMock.mock.calls[0][0] as string;
    expect(calledUrl).toBe(`${baseUrl}/api/messages/sent?page=2&pageSize=10`);
    expect(result.page).toBe(2);
    expect(result.hasMore).toBe(true);
  });

  it("markMessageRead sends POST to the read endpoint", async () => {
    fetchMock.mockResolvedValue(jsonResponse(204));

    await markMessageRead(sampleMessage.id);

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/messages/${sampleMessage.id}/read`);
    expect(init.method).toBe("POST");
  });

  it("getUnreadCount returns the count", async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, { data: 4, meta: null }));

    const result = await getUnreadCount();

    const calledUrl = fetchMock.mock.calls[0][0] as string;
    expect(calledUrl).toBe(`${baseUrl}/api/messages/unread-count`);
    expect(result).toBe(4);
  });
});
