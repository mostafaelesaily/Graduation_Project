import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/Home/Presentation/Models/home_models.dart';

class TopProvidersSection extends StatelessWidget {
  final List<FeaturedProvider> providers;
  final String area;
  final ValueChanged<FeaturedProvider>? onBookSlot;

  const TopProvidersSection({
    super.key,
    required this.providers,
    this.area = 'Downtown',
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
          'Available today around $area',
          style: TextStyle(color: ColorsManager.fadedText, fontSize: 14.sp),
        ),
        SizedBox(height: 12.h),
        SizedBox(
          height: 292.h,
          child: ListView.separated(
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
          ),
        ),
      ],
    );
  }
}

class _ProviderCard extends StatelessWidget {
  final FeaturedProvider provider;
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
          ClipRRect(
            borderRadius: BorderRadius.circular(16.r),
            child: Stack(
              children: [
                Container(
                  height: 108.h,
                  width: double.infinity,
                  decoration: BoxDecoration(
                    gradient: LinearGradient(
                      begin: Alignment.topLeft,
                      end: Alignment.bottomRight,
                      colors: [
                        ColorsManager.primaryColor.withValues(alpha: 0.35),
                        ColorsManager.accentTeal.withValues(alpha: 0.35),
                      ],
                    ),
                  ),
                  child: Icon(
                    Icons.person,
                    color: Colors.white.withValues(alpha: 0.9),
                    size: 60.sp,
                  ),
                ),
                Positioned(
                  top: 8.h,
                  left: 8.w,
                  child: _Badge(
                    leading: Icon(
                      Icons.star_rounded,
                      color: ColorsManager.star,
                      size: 14.sp,
                    ),
                    text: '${provider.rating} (${provider.reviewsCount})',
                    textColor: ColorsManager.textDark,
                    background: Colors.white.withValues(alpha: 0.92),
                  ),
                ),
                Positioned(
                  bottom: 8.h,
                  right: 8.w,
                  child: _Badge(
                    text: '${provider.distanceMiles} mi',
                    textColor: Colors.white,
                    background: ColorsManager.primaryColor,
                  ),
                ),
              ],
            ),
          ),
          SizedBox(height: 10.h),
          Text(
            provider.name,
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
            provider.specialty,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: TextStyle(color: ColorsManager.fadedText, fontSize: 13.sp),
          ),
          SizedBox(height: 10.h),
          Container(
            width: double.infinity,
            padding: EdgeInsets.symmetric(horizontal: 10.w, vertical: 8.h),
            decoration: BoxDecoration(
              color: ColorsManager.primaryBackgroundColor,
              borderRadius: BorderRadius.circular(10.r),
            ),
            child: Row(
              children: [
                Icon(
                  Icons.event_available_outlined,
                  color: ColorsManager.primaryColor,
                  size: 16.sp,
                ),
                SizedBox(width: 6.w),
                Expanded(
                  child: Text(
                    'Next: ${provider.nextSlot}',
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: TextStyle(
                      color: ColorsManager.primaryColor,
                      fontSize: 13.sp,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
              ],
            ),
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

class _Badge extends StatelessWidget {
  final Widget? leading;
  final String text;
  final Color textColor;
  final Color background;

  const _Badge({
    this.leading,
    required this.text,
    required this.textColor,
    required this.background,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.symmetric(horizontal: 8.w, vertical: 4.h),
      decoration: BoxDecoration(
        color: background,
        borderRadius: BorderRadius.circular(20.r),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          if (leading != null) ...[leading!, SizedBox(width: 3.w)],
          Text(
            text,
            style: TextStyle(
              color: textColor,
              fontSize: 12.sp,
              fontWeight: FontWeight.bold,
            ),
          ),
        ],
      ),
    );
  }
}
