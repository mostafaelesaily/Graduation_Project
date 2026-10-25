enum LoginStatus { initial, loading, success, failure }

class LoginState {
  final LoginStatus status;
  final Map<String, dynamic>? errorMessage;

  const LoginState({this.status = LoginStatus.initial, this.errorMessage});

  LoginState copyWith({
    LoginStatus? status,
    Map<String, dynamic>? errorMessage,
    bool clearError = false,
  }) {
    return LoginState(
      status: status ?? this.status,
      errorMessage: clearError ? null : errorMessage ?? this.errorMessage,
    );
  }
}
