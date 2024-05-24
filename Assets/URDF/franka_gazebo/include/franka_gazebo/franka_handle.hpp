#pragma once

#include <ros/callback_queue.h>
#include <ros/ros.h>
#include <ros/subscribe_options.h>

#include <gazebo/physics/physics.hh>
#include <gazebo/transport/TransportTypes.hh>
#include <gazebo/common/Plugin.hh>
#include <gazebo/common/Events.hh>

#include <geometry_msgs/WrenchStamped.h>
#include <boost/thread.hpp>
#include <boost/thread/mutex.hpp>
#include <memory>

namespace gazebo
{

class ForcePlugin : public ModelPlugin
{

    public: 
    
    ForcePlugin();

    virtual ~ForcePlugin();
    
    void Load( physics::ModelPtr _parent, sdf::ElementPtr );

    protected:
    
    virtual void Update();

    virtual void UpdateBodyForce(const geometry_msgs::Wrench::ConstPtr & _msg);

    private:

    double update_rate_;

    std::string robot_namespace_;
    std::string topic_name_;
    std::string link_name_;

    physics::WorldPtr world_;
    physics::LinkPtr link_;

    std::unique_ptr<ros::NodeHandle> rosnode_;
    ros::Publisher pub_;
    ros::Subscriber sub_;

    geometry_msgs::Wrench wrench_msg_;
    
    void QueueThread();

    boost::mutex lock_;
    ros::CallbackQueue queue_;
    boost::thread callback_queue_thread_;
    event::ConnectionPtr update_connection_;

};

}