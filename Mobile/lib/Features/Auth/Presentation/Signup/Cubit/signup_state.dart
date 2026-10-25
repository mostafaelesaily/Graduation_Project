enum SignupStatus { initial, loading, success, failure }

class SignupState {
  final SignupStatus status;
  final String? errorMessage;

  const SignupState({this.status = SignupStatus.initial, this.errorMessage});

  SignupState copyWith({
    SignupStatus? status,
    String? errorMessage,
    bool clearError = false,
  }) {
    return SignupState(
      status: status ?? this.status,
      errorMessage: clearError ? null : errorMessage ?? this.errorMessage,
    );
  }
}
