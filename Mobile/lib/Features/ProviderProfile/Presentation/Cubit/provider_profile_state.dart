enum BookingStatus { initial, loading, success, failure }

class ProviderProfileState {
  final BookingStatus status;
  final Map<String, dynamic>? errorMessage;

  const ProviderProfileState({
    this.status = BookingStatus.initial,
    this.errorMessage,
  });

  ProviderProfileState copyWith({
    BookingStatus? status,
    Map<String, dynamic>? errorMessage,
    bool clearError = false,
  }) {
    return ProviderProfileState(
      status: status ?? this.status,
      errorMessage: clearError ? null : errorMessage ?? this.errorMessage,
    );
  }
}
