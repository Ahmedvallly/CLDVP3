using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using KhumaloCraftFinal.Models;
using KhumaloCraftFinal.Views.Orders;  // Make sure this namespace matches your project structure

namespace KhumaloCraftFinal.Services
{
    public class AzureFunctionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _functionBaseUrl;

        public AzureFunctionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _functionBaseUrl = configuration["AzureFunctionBaseUrl"];
        }

        // Notification Functions
        public async Task<List<Notification>> GetNotificationsAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"{_functionBaseUrl}/api/notifications/{userId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Notification>>();
        }

        public async Task ClearAllNotificationsAsync(int userId)
        {
            var response = await _httpClient.PostAsync($"{_functionBaseUrl}/api/notifications/clear/{userId}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task ClearSingleNotificationAsync(int userId, int notificationId)
        {
            var response = await _httpClient.PostAsync($"{_functionBaseUrl}/api/notifications/clear/{userId}/{notificationId}", null);
            response.EnsureSuccessStatusCode();
        }

        // Order Functions
        public async Task<Order> ProcessOrderAsync(OrderInput orderInput)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_functionBaseUrl}/api/orders/process", orderInput);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>();
        }

        public async Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var statusUpdate = new OrderStatusUpdate { NewStatus = newStatus };
            var response = await _httpClient.PostAsJsonAsync($"{_functionBaseUrl}/api/orders/{orderId}/status", statusUpdate);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>();
        }

        public async Task<List<Order>> GetPastOrdersAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"{_functionBaseUrl}/api/orders/past/{userId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Order>>();
        }

        public async Task<List<Order>> GetAdminOrdersAsync()
        {
            var response = await _httpClient.GetAsync($"{_functionBaseUrl}/api/orders/admin");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Order>>();
        }
    }

    public class OrderInput
    {
        public int UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; }
        // Add other necessary properties
    }

    public class OrderStatusUpdate
    {
        public OrderStatus NewStatus { get; set; }
    }
}