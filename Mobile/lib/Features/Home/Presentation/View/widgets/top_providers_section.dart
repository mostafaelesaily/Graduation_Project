import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Entites/provider_profile.dart';

class TopProvidersSection extends StatelessWidget {
  final List<ProviderProfile> providers;
  final bool isLoading;
  final bool hasError;
  final VoidCallback? onRetry;
  final ValueChanged<ProviderProfile>? onBookSlot;

  const TopProvidersSection({
    super.key,
    required this.providers,
    this.isLoading = false,
    this.hasError = false,
    this.onRetry,
    this.onBookSlot,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Top Verified Providers',
          style: TextStyle(
            color: ColorsManager.textDark,
            fontSize: 20.sp,
            fontWeight: FontWeight.bold,
          ),
        ),
        SizedBox(height: 2.h),
        Text(
          'Verified providers around Cairo',
          style: TextStyle(color: ColorsManager.fadedText, fontSize: 14.sp),
        ),
        SizedBox(height: 12.h),
        SizedBox(height: 232.h, child: _buildContent()),
      ],
    );
  }

  Widget _buildContent() {
    if (isLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (hasError) {
      return Center(
        child: TextButton(
          onPressed: onRetry,
          child: const Text('Could not load providers. Tap to retry.'),
        ),
      );
    }

    if (providers.isEmpty) {
      return const Center(child: Text('No providers found.'));
    }

    return ListView.separated(
      scrollDirection: Axis.horizontal,
      clipBehavior: Clip.none,
      itemCount: providers.length,
      separatorBuilder: (_, _) => SizedBox(width: 14.w),
      itemBuilder: (context, index) {
        final provider = providers[index];
        return _ProviderCard(
          provider: provider,
          onBookSlot: () => onBookSlot?.call(provider),
        );
      },
    );
  }
}

class _ProviderCard extends StatelessWidget {
  final ProviderProfile provider;
  final VoidCallback onBookSlot;

  const _ProviderCard({required this.provider, required this.onBookSlot});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 240.w,
      padding: EdgeInsets.all(12.w),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24.r),
        border: Border.all(color: ColorsManager.cardBorder),
        boxShadow: [
          BoxShadow(
            color: ColorsManager.primaryColor.withValues(alpha: 0.05),
            blurRadius: 20,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            height: 80.h,
            width: double.infinity,
            decoration: BoxDecoration(
              color: ColorsManager.softBlue,
              borderRadius: BorderRadius.circular(16.r),
            ),
            child: Icon(
              Icons.local_hospital_outlined,
              color: ColorsManager.primaryColor,
              size: 40.sp,
            ),
          ),
          SizedBox(height: 10.h),
          Text(
            provider.serviceName,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: TextStyle(
              color: ColorsManager.textDark,
              fontSize: 17.sp,
              fontWeight: FontWeight.bold,
            ),
          ),
          SizedBox(height: 2.h),
          Text(
            provider.address,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: TextStyle(color: ColorsManager.fadedText, fontSize: 13.sp),
          ),
          const Spacer(),
          SizedBox(
            width: double.infinity,
            height: 44.h,
            child: OutlinedButton(
              onPressed: onBookSlot,
              style: OutlinedButton.styleFrom(
                foregroundColor: ColorsManager.primaryColor,
                backgroundColor: ColorsManager.softBlue,
                side: BorderSide(color: ColorsManager.cardBorder),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(14.r),
                ),
              ),
              child: Text(
                'Book Slot',
                style: TextStyle(fontSize: 15.sp, fontWeight: FontWeight.bold),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
