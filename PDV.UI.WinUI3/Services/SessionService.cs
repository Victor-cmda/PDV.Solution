using PDV.Domain.Entities;
using PDV.Domain.Interfaces;
using System;

namespace PDV.UI.WinUI3.Services
{
    /// <summary>
    /// Serviço para gerenciar a sessão do usuário
    /// </summary>
    public class SessionService
    {
        private static SessionService _instance;
        private Employee _currentUser;

        // Eventos
        public event EventHandler UserLoggedIn;
        public event EventHandler UserLoggedOut;

        private SessionService() { }

        public static SessionService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SessionService();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Usuário atual logado no sistema
        /// </summary>
        public Employee CurrentUser
        {
            get => _currentUser;
            set
            {
                bool wasLoggedIn = _currentUser != null;
                _currentUser = value;

                if (_currentUser != null && !wasLoggedIn)
                {
                    // Usuário acabou de fazer login
                    UserLoggedIn?.Invoke(this, EventArgs.Empty);
                }
                else if (_currentUser == null && wasLoggedIn)
                {
                    // Usuário acabou de fazer logout
                    UserLoggedOut?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Verifica se há um usuário autenticado
        /// </summary>
        public bool IsAuthenticated => _currentUser != null;

        /// <summary>
        /// Verifica se o usuário atual tem uma permissão específica
        /// </summary>
        public bool HasPermission(string permission)
        {
            if (_currentUser == null)
                return false;

            return _currentUser.Permissions?.Contains(permission) ?? false;
        }

        /// <summary>
        /// Limpa a sessão atual (logout)
        /// </summary>
        public void Logout()
        {
            CurrentUser = null;
        }
    }
}